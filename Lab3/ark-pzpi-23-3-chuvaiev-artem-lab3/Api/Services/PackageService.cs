using Api.Infrastructure.Errors;
using Api.Infrastructure.Repository;
using Api.Infrastructure.ResultPattern;
using Api.Models;
using Api.Models.DTOs;
using Api.Services.Interfaces;
using Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Services;

public class PackageService : IPackageService
{
    private readonly IGenericRepository<Package> _packageRepository;
    private readonly IGenericRepository<DeliveryStatus> _deliveryStatusRepository;
    private readonly IGenericRepository<PackageCategory> _packageCategoryRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUserService _userService;
    private readonly INfcService _nfcService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PackageService> _logger;

    public PackageService(
        IGenericRepository<Package> packageRepository,
        IGenericRepository<DeliveryStatus> deliveryStatusRepository,
        IGenericRepository<PackageCategory> packageCategoryRepository,
        IGenericRepository<User> userRepository,
        IUserService userService,
        INfcService nfcService,
        IEmailService emailService,
        ILogger<PackageService> logger)
    {
        _packageRepository = packageRepository;
        _deliveryStatusRepository = deliveryStatusRepository;
        _packageCategoryRepository = packageCategoryRepository;
        _userRepository = userRepository;
        _userService = userService;
        _nfcService = nfcService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<PackageDto>>> GetPackagesForCourierAsync(string serialNumber)
    {
        // Get user by NFC serial number
        var userIdResult = await _nfcService.GetUserIdByNfcSerialAsync(serialNumber);
        if (!userIdResult.IsSuccess)
        {
            return Result<IEnumerable<PackageDto>>.Failure(userIdResult.Errors);
        }

        // Get user
        if (!userIdResult.Value.HasValue)
        {
            return Result<IEnumerable<PackageDto>>.Failure(NfcErrors.NfcNotFound());
        }

        var userResult = await _userService.GetUserByIdAsync(userIdResult.Value.Value);
        if (!userResult.IsSuccess)
        {
            return Result<IEnumerable<PackageDto>>.Failure(userResult.Errors);
        }

        // Check if user is courier
        var isCourier = userResult.Value.Roles?.Any(r => r.Role?.Name == "Courier") ?? false;
        if (!isCourier)
        {
            return Result<IEnumerable<PackageDto>>.Failure(Error.Forbidden("package.NOT_COURIER",
                "User is not a courier"));
        }

        // Get all packages with Pending status
        var pendingStatus = await _deliveryStatusRepository.GetSingleByConditionAsync(s => s.Name == "Pending");
        if (!pendingStatus.IsSuccess)
        {
            return Result<IEnumerable<PackageDto>>.Failure(Error.NotFound("status.NOT_FOUND",
                "Pending status not found"));
        }

        // Note: We need to load DeliveryStatus for comparison
        // In a real scenario, we'd use DeliveryStatusId foreign key
        // For now, we'll get all packages and filter by status name after loading
        var allPackagesResult =
            await _packageRepository.GetListByConditionAsync(includes: [s => s.Include(c => c.Category)]);
        if (!allPackagesResult.IsSuccess)
        {
            return Result<IEnumerable<PackageDto>>.Failure(allPackagesResult.Errors);
        }

        // Filter packages with Pending status
        var packages = allPackagesResult.Value.Where(p =>
            p.DeliveryStatus != null && p.DeliveryStatus.Name == "Pending").ToList();

        var packageDtos = packages.Select(p => new PackageDto
        {
            Id = p.Id,
            Height = p.Height,
            Width = p.Width,
            Depth = p.Depth,
            PostBoxId = p.PostBoxId,
            CategoryName = p.Category?.Name ?? "",
            DeliveryStatusName = p.DeliveryStatus?.Name ?? "",
            CreatedOn = p.CreatedOn
        });

        return Result<IEnumerable<PackageDto>>.Success(packageDtos);
    }

    public async Task<Result<IEnumerable<PackageDto>>> GetPackagesForClientAsync(int userId)
    {
        // Get packages directly by UserId
        var packagesResult = await _packageRepository.GetListByConditionAsync(
            p => p.UserId == userId,
            includes: [query => query.Include(p => p.Category).Include(p => p.DeliveryStatus)]);

        if (!packagesResult.IsSuccess)
        {
            return Result<IEnumerable<PackageDto>>.Failure(packagesResult.Errors);
        }

        var packageDtos = packagesResult.Value.Select(p => new PackageDto
        {
            Id = p.Id,
            Height = p.Height,
            Width = p.Width,
            Depth = p.Depth,
            PostBoxId = p.PostBoxId,
            CategoryName = p.Category?.Name ?? "",
            DeliveryStatusName = p.DeliveryStatus?.Name ?? "",
            CreatedOn = p.CreatedOn
        });

        return Result<IEnumerable<PackageDto>>.Success(packageDtos);
    }

    public async Task<Result> ChangePackageStatusToInProgressAsync(int packageId, string serialNumber)
    {
        // Validate NFC - check if serial number exists
        var userIdResult = await _nfcService.GetUserIdByNfcSerialAsync(serialNumber);
        if (!userIdResult.IsSuccess)
        {
            return Result.Failure(userIdResult.Errors);
        }

        // Get package
        var packageResult = await GetPackageByIdAsync(packageId);
        if (!packageResult.IsSuccess)
        {
            return Result.Failure(packageResult.Errors);
        }

        // Check if status is Pending
        if (packageResult.Value.DeliveryStatus?.Name != "Pending")
        {
            return Result.Failure(PackageErrors.InvalidStatusTransition());
        }

        // Get InProgress status
        var inProgressStatus = await _deliveryStatusRepository.GetSingleByConditionAsync(s => s.Name == "In Progress");
        if (!inProgressStatus.IsSuccess)
        {
            return Result.Failure(Error.NotFound("status.NOT_FOUND", "InProgress status not found"));
        }

        // Update status
        packageResult.Value.DeliveryStatus = inProgressStatus.Value;
        var updateResult = await _packageRepository.UpdateAsync(packageResult.Value);
        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.Errors);
        }

        _logger.LogInformation("Package {PackageId} status changed to In Progress", packageId);

        return Result.Success();
    }

    public async Task<Result> PlacePackageInLockerAsync(PlacePackageDto dto)
    {
        // Validate NFC - check if serial number exists
        var userIdResult = await _nfcService.GetUserIdByNfcSerialAsync(dto.SerialNumber);
        if (!userIdResult.IsSuccess)
        {
            return Result.Failure(userIdResult.Errors);
        }

        // Get package
        var packageResult = await GetPackageByIdAsync(dto.PackageId);
        if (!packageResult.IsSuccess)
        {
            return Result.Failure(packageResult.Errors);
        }

        // Check if locker is available (simplified - in real system would check locker status)
        // For now, just update package
        packageResult.Value.PostBoxId = dto.PostBoxId;

        // Get Delivered status
        var deliveredStatus = await _deliveryStatusRepository.GetSingleByConditionAsync(s => s.Name == "Delivered");
        if (!deliveredStatus.IsSuccess)
        {
            return Result.Failure(Error.NotFound("status.NOT_FOUND", "Delivered status not found"));
        }

        packageResult.Value.DeliveryStatus = deliveredStatus.Value;

        var updateResult = await _packageRepository.UpdateAsync(packageResult.Value);
        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.Errors);
        }

        // Get user for email notification
        var userResult = await _userService.GetUserByIdAsync(packageResult.Value.UserId);
        if (userResult.IsSuccess)
        {
            await _emailService.SendSuccessfulEmailAsync(
                userResult.Value.EmailAddress,
                $"Your package has been placed in locker {dto.PostBoxId}. You can pick it up using your NFC card.",
                "Package Ready for Pickup");
        }

        _logger.LogInformation("Package {PackageId} placed in locker {PostBoxId}", dto.PackageId, dto.PostBoxId);

        return Result.Success();
    }

    public async Task<Result> ReceivePackageAsync(int packageId, string serialNumber)
    {
        // Get user by NFC serial number
        var userIdResult = await _nfcService.GetUserIdByNfcSerialAsync(serialNumber);
        if (!userIdResult.IsSuccess || !userIdResult.Value.HasValue)
        {
            return Result.Failure(userIdResult.IsSuccess ? NfcErrors.NfcNotFound() : userIdResult.Errors.First());
        }

        var userId = userIdResult.Value.Value;

        // Get package
        var packageResult = await GetPackageByIdAsync(packageId);
        if (!packageResult.IsSuccess)
        {
            return Result.Failure(packageResult.Errors);
        }

        // Check locker binding
        var bindingResult = await CheckLockerToClientBindingAsync(packageResult.Value.PostBoxId, serialNumber, packageId);
        if (!bindingResult.IsSuccess || !bindingResult.Value)
        {
            return Result.Failure(PackageErrors.LockerNotBoundToClient());
        }

        // Get Received status
        var receivedStatus = await _deliveryStatusRepository.GetSingleByConditionAsync(s => s.Name == "Received");
        if (!receivedStatus.IsSuccess)
        {
            return Result.Failure(Error.NotFound("status.NOT_FOUND", "Received status not found"));
        }

        // Update status
        packageResult.Value.DeliveryStatus = receivedStatus.Value;
        var updateResult = await _packageRepository.UpdateAsync(packageResult.Value);
        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.Errors);
        }

        // Get user for email confirmation
        var userResult = await _userService.GetUserByIdAsync(packageResult.Value.UserId);
        if (userResult.IsSuccess)
        {
            await _emailService.SendSuccessfulEmailAsync(
                userResult.Value.EmailAddress,
                "Your package has been successfully received. Thank you for using our service!",
                "Package Received");
        }

        _logger.LogInformation("Package {PackageId} received by user with NFC {SerialNumber}", packageId, serialNumber);

        return Result.Success();
    }

    public async Task<Result<bool>> CheckLockerToClientBindingAsync(int postBoxId, string serialNumber, int packageId)
    {
        // Get user by NFC serial number
        var userIdResult = await _nfcService.GetUserIdByNfcSerialAsync(serialNumber);
        if (!userIdResult.IsSuccess || !userIdResult.Value.HasValue)
        {
            return Result<bool>.Failure(userIdResult.IsSuccess ? NfcErrors.NfcNotFound() : userIdResult.Errors.First());
        }

        var userId = userIdResult.Value.Value;

        // Get package in this locker
        // First get all packages in this locker, then filter by status
        var allPackagesResult = await _packageRepository.GetListByConditionAsync(
            p => p.PostBoxId == postBoxId, includes: [s => s.Include(q => q.DeliveryStatus)]);

        if (!allPackagesResult.IsSuccess)
        {
            return Result<bool>.Failure(allPackagesResult.Errors);
        }

        var package = allPackagesResult.Value.FirstOrDefault(
            p => p.DeliveryStatus != null && p.Id == packageId && p.DeliveryStatus.Name == "Delivered");

        if (package == null)
        {
            return Result<bool>.Failure(PackageErrors.PackageNotFound());
        }

        // Check if package belongs to user with this NFC
        return Result<bool>.Success(package.UserId == userId);
    }

    public async Task<Result<Package>> GetPackageByIdAsync(int packageId)
    {
        var result = await _packageRepository.GetSingleByConditionAsync(p => p.Id == packageId,
            [s => s.Include(q => q.DeliveryStatus), s => s.Include(q => q.Category)]);

        if (!result.IsSuccess)
        {
            return Result<Package>.Failure(PackageErrors.PackageNotFound());
        }

        return Result<Package>.Success(result.Value);
    }

    public async Task<Result> UpdatePackageStatusAsync(int packageId, int statusId)
    {
        var packageResult = await GetPackageByIdAsync(packageId);
        if (!packageResult.IsSuccess)
        {
            return Result.Failure(packageResult.Errors);
        }

        var statusResult = await _deliveryStatusRepository.GetSingleByConditionAsync(s => s.Id == statusId);
        if (!statusResult.IsSuccess)
        {
            return Result.Failure(Error.NotFound("status.NOT_FOUND", "Status not found"));
        }

        packageResult.Value.DeliveryStatus = statusResult.Value;
        var updateResult = await _packageRepository.UpdateAsync(packageResult.Value);
        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.Errors);
        }

        return Result.Success();
    }

    public async Task<Result<IEnumerable<LockerPackageDto>>> OpenAllDeliveredLockersForUserAsync(string serialNumber)
    {
        // Get user by NFC serial number
        var userIdResult = await _nfcService.GetUserIdByNfcSerialAsync(serialNumber);
        if (!userIdResult.IsSuccess || !userIdResult.Value.HasValue)
        {
            return Result<IEnumerable<LockerPackageDto>>.Failure(
                userIdResult.IsSuccess ? NfcErrors.NfcNotFound() : userIdResult.Errors.First());
        }

        var userId = userIdResult.Value.Value;

        // Get all packages with Delivered status for this user
        var allPackagesResult = await _packageRepository.GetListByConditionAsync(
            p => p.UserId == userId,
            includes: [query => query.Include(p => p.DeliveryStatus)]);

        if (!allPackagesResult.IsSuccess)
        {
            return Result<IEnumerable<LockerPackageDto>>.Failure(allPackagesResult.Errors);
        }

        // Filter packages with Delivered status
        var deliveredPackages = allPackagesResult.Value
            .Where(p => p.DeliveryStatus != null && p.DeliveryStatus.Name == "Delivered" && p.PostBoxId > 0)
            .ToList();

        if (!deliveredPackages.Any())
        {
            return Result<IEnumerable<LockerPackageDto>>.Failure(
                Error.NotFound("package.NO_DELIVERED_PACKAGES", "No delivered packages found for this user"));
        }

        // Return locker IDs with package IDs (one package per locker)
        var lockerPackages = deliveredPackages
            .Select(p => new LockerPackageDto
            {
                LockerId = p.PostBoxId,
                PackageId = p.Id
            })
            .ToList();

        _logger.LogInformation(
            "Opening {Count} lockers for user {UserId} with NFC {SerialNumber}",
            lockerPackages.Count, userId, serialNumber);

        return Result<IEnumerable<LockerPackageDto>>.Success(lockerPackages);
    }
}