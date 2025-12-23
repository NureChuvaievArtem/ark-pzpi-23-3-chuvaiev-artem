using Api.Infrastructure.ResultPattern;
using Api.Models;
using Api.Models.DTOs;

namespace Api.Services.Interfaces;

public interface IPackageService
{
    Task<Result<IEnumerable<PackageDto>>> GetPackagesForCourierAsync(string serialNumber);
    
    Task<Result<IEnumerable<PackageDto>>> GetPackagesForClientAsync(int userId);
    
    Task<Result> ChangePackageStatusToInProgressAsync(int packageId, string serialNumber);
    
    Task<Result> PlacePackageInLockerAsync(PlacePackageDto dto);
    
    Task<Result> ReceivePackageAsync(int packageId, string serialNumber);
    
    Task<Result<bool>> CheckLockerToClientBindingAsync(int postBoxId, string serialNumber);
    
    Task<Result<Package>> GetPackageByIdAsync(int packageId);
    Task<Result> UpdatePackageStatusAsync(int packageId, int statusId);
    
    Task<Result<IEnumerable<int>>> OpenAllDeliveredLockersForUserAsync(string serialNumber);
}

