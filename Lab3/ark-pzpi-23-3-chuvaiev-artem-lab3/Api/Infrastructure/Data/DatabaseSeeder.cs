using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;

    public DatabaseSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if PackageCategories table is empty
        if (await _context.PackageCategories.AnyAsync())
        {
            return; // Database already seeded
        }

        var now = DateTimeOffset.UtcNow;

        // Seed Roles
        var clientRole = new Role
        {
            Name = "Client",
            CreatedOn = now,
            LastModifiedOn = now
        };

        var courierRole = new Role
        {
            Name = "Courier",
            CreatedOn = now,
            LastModifiedOn = now
        };

        _context.Roles.AddRange(clientRole, courierRole);
        await _context.SaveChangesAsync();

        // Seed PackageCategories
        var packageCategories = new List<PackageCategory>
        {
            new PackageCategory
            {
                Name = "Electronics",
                IsFragile = true,
                CreatedOn = now,
                LastModifiedOn = now
            },
            new PackageCategory
            {
                Name = "Clothing",
                IsFragile = false,
                CreatedOn = now,
                LastModifiedOn = now
            },
            new PackageCategory
            {
                Name = "Glassware",
                IsFragile = true,
                CreatedOn = now,
                LastModifiedOn = now
            },
            new PackageCategory
            {
                Name = "Books",
                IsFragile = false,
                CreatedOn = now,
                LastModifiedOn = now
            },
            new PackageCategory
            {
                Name = "Furniture",
                IsFragile = false,
                CreatedOn = now,
                LastModifiedOn = now
            }
        };

        _context.PackageCategories.AddRange(packageCategories);
        await _context.SaveChangesAsync();

        // Seed DeliveryStatuses
        var deliveryStatuses = new List<DeliveryStatus>
        {
            new DeliveryStatus
            {
                Name = "Pending",
                CreatedOn = now,
                LastModifiedOn = now
            },
            new DeliveryStatus
            {
                Name = "In Progress",
                CreatedOn = now,
                LastModifiedOn = now
            },
            new DeliveryStatus
            {
                Name = "Delivered",
                CreatedOn = now,
                LastModifiedOn = now
            },
            new DeliveryStatus
            {
                Name = "Received",
                CreatedOn = now,
                LastModifiedOn = now
            }
        };

        _context.DeliveryStatuses.AddRange(deliveryStatuses);
        await _context.SaveChangesAsync();

        // Seed Test Users
        var testUsers = new List<User>
        {
            new User
            {
                EmailAddress = "john.doe@example.com",
                SerialNfcData = null,
                CreatedOn = now,
                LastModifiedOn = now
            },
            new User
            {
                EmailAddress = "jane.smith@example.com",
                SerialNfcData = null,
                CreatedOn = now,
                LastModifiedOn = now
            },
            new User
            {
                EmailAddress = "bob.johnson@example.com",
                SerialNfcData = null,
                CreatedOn = now,
                LastModifiedOn = now
            },
            new User
            {
                EmailAddress = "alice.williams@example.com",
                SerialNfcData = null,
                CreatedOn = now,
                LastModifiedOn = now
            },
            new User
            {
                EmailAddress = "charlie.brown@example.com",
                SerialNfcData = null,
                CreatedOn = now,
                LastModifiedOn = now
            }
        };

        _context.Users.AddRange(testUsers);
        await _context.SaveChangesAsync();

        // Assign Client role to all test users
        var userRoles = testUsers.Select(user => new UserRole
        {
            UserId = user.Id,
            RoleId = clientRole.Id,
            CreatedOn = now,
            LastModifiedOn = now
        }).ToList();

        _context.UserRoles.AddRange(userRoles);
        await _context.SaveChangesAsync();

        // Seed Packages with realistic data for analytics
        {
            var electronicsCategory = packageCategories[0]; // Electronics
            var clothingCategory = packageCategories[1]; // Clothing
            var glasswareCategory = packageCategories[2]; // Glassware
            var booksCategory = packageCategories[3]; // Books
            var furnitureCategory = packageCategories[4]; // Furniture

            var pendingStatus = deliveryStatuses[0]; // Pending
            var inProgressStatus = deliveryStatuses[1]; // In Progress
            var deliveredStatus = deliveryStatuses[2]; // Delivered
            var receivedStatus = deliveryStatuses[3]; // Received

            // Create packages with varied dates over the last 30 days for time series analysis
            var random = new Random(42); // Fixed seed for reproducibility
            var baseDate = DateTimeOffset.UtcNow.AddDays(-30);
            
            var packages = new List<Package>();

            // Generate packages with realistic distribution
            // Small packages (volumes 500-2000)
            for (int i = 0; i < 15; i++)
            {
                var createdDate = baseDate.AddDays(random.Next(0, 30))
                    .AddHours(random.Next(8, 20)) // Business hours
                    .AddMinutes(random.Next(0, 60));
                
                var deliveryHours = random.Next(2, 48); // 2-48 hours delivery time
                var lastModified = createdDate.AddHours(deliveryHours);
                
                var status = random.Next(100) switch
                {
                    < 20 => pendingStatus,
                    < 50 => inProgressStatus,
                    < 85 => deliveredStatus,
                    _ => receivedStatus
                };

                packages.Add(new Package
                {
                    UserId = testUsers[random.Next(testUsers.Count)].Id,
                    Height = random.Next(8, 15),
                    Width = random.Next(8, 15),
                    Depth = random.Next(5, 12),
                    PostBoxId = random.Next(1, 8),
                    Category = random.Next(100) < 30 ? electronicsCategory : 
                               random.Next(100) < 60 ? clothingCategory : booksCategory,
                    DeliveryStatus = status,
                    CreatedOn = createdDate,
                    LastModifiedOn = status == pendingStatus ? createdDate : lastModified
                });
            }

            // Medium packages (volumes 2000-8000)
            for (int i = 0; i < 20; i++)
            {
                var createdDate = baseDate.AddDays(random.Next(0, 30))
                    .AddHours(random.Next(8, 20))
                    .AddMinutes(random.Next(0, 60));
                
                var deliveryHours = random.Next(6, 72); // 6-72 hours
                var lastModified = createdDate.AddHours(deliveryHours);
                
                var status = random.Next(100) switch
                {
                    < 15 => pendingStatus,
                    < 40 => inProgressStatus,
                    < 80 => deliveredStatus,
                    _ => receivedStatus
                };

                packages.Add(new Package
                {
                    UserId = testUsers[random.Next(testUsers.Count)].Id,
                    Height = random.Next(15, 25),
                    Width = random.Next(15, 25),
                    Depth = random.Next(10, 18),
                    PostBoxId = random.Next(1, 8),
                    Category = random.Next(100) < 25 ? electronicsCategory : 
                               random.Next(100) < 50 ? clothingCategory :
                               random.Next(100) < 75 ? booksCategory : glasswareCategory,
                    DeliveryStatus = status,
                    CreatedOn = createdDate,
                    LastModifiedOn = status == pendingStatus ? createdDate : lastModified
                });
            }

            // Large packages (volumes 8000-20000)
            for (int i = 0; i < 12; i++)
            {
                var createdDate = baseDate.AddDays(random.Next(0, 30))
                    .AddHours(random.Next(8, 20))
                    .AddMinutes(random.Next(0, 60));
                
                var deliveryHours = random.Next(12, 120); // 12-120 hours
                var lastModified = createdDate.AddHours(deliveryHours);
                
                var status = random.Next(100) switch
                {
                    < 20 => pendingStatus,
                    < 45 => inProgressStatus,
                    < 85 => deliveredStatus,
                    _ => receivedStatus
                };

                packages.Add(new Package
                {
                    UserId = testUsers[random.Next(testUsers.Count)].Id,
                    Height = random.Next(25, 35),
                    Width = random.Next(25, 35),
                    Depth = random.Next(18, 28),
                    PostBoxId = random.Next(1, 8),
                    Category = random.Next(100) < 30 ? furnitureCategory : 
                               random.Next(100) < 60 ? electronicsCategory : glasswareCategory,
                    DeliveryStatus = status,
                    CreatedOn = createdDate,
                    LastModifiedOn = status == pendingStatus ? createdDate : lastModified
                });
            }

            // Very large packages (outliers for distribution analysis)
            for (int i = 0; i < 5; i++)
            {
                var createdDate = baseDate.AddDays(random.Next(0, 30))
                    .AddHours(random.Next(8, 20))
                    .AddMinutes(random.Next(0, 60));
                
                var deliveryHours = random.Next(24, 168); // 1-7 days
                var lastModified = createdDate.AddHours(deliveryHours);
                
                var status = random.Next(100) < 30 ? inProgressStatus : deliveredStatus;

                packages.Add(new Package
                {
                    UserId = testUsers[random.Next(testUsers.Count)].Id,
                    Height = random.Next(40, 60),
                    Width = random.Next(40, 60),
                    Depth = random.Next(30, 50),
                    PostBoxId = random.Next(1, 8),
                    Category = furnitureCategory,
                    DeliveryStatus = status,
                    CreatedOn = createdDate,
                    LastModifiedOn = lastModified
                });
            }

            // Very small packages (outliers on the other end)
            for (int i = 0; i < 8; i++)
            {
                var createdDate = baseDate.AddDays(random.Next(0, 30))
                    .AddHours(random.Next(8, 20))
                    .AddMinutes(random.Next(0, 60));
                
                var deliveryHours = random.Next(1, 24); // Fast delivery
                var lastModified = createdDate.AddHours(deliveryHours);
                
                var status = random.Next(100) < 40 ? deliveredStatus : receivedStatus;

                packages.Add(new Package
                {
                    UserId = testUsers[random.Next(testUsers.Count)].Id,
                    Height = random.Next(3, 8),
                    Width = random.Next(3, 8),
                    Depth = random.Next(2, 6),
                    PostBoxId = random.Next(1, 8),
                    Category = random.Next(100) < 50 ? electronicsCategory : booksCategory,
                    DeliveryStatus = status,
                    CreatedOn = createdDate,
                    LastModifiedOn = lastModified
                });
            }

            // Add some packages with specific patterns for analysis:
            // - Weekend packages (Saturday/Sunday)
            for (int i = 0; i < 5; i++)
            {
                var dayOffset = random.Next(0, 4) * 7 + random.Next(5, 7); // Weekend days
                var createdDate = baseDate.AddDays(dayOffset)
                    .AddHours(random.Next(10, 18))
                    .AddMinutes(random.Next(0, 60));
                
                var deliveryHours = random.Next(12, 72);
                var lastModified = createdDate.AddHours(deliveryHours);
                
                packages.Add(new Package
                {
                    UserId = testUsers[random.Next(testUsers.Count)].Id,
                    Height = random.Next(12, 22),
                    Width = random.Next(12, 22),
                    Depth = random.Next(8, 15),
                    PostBoxId = random.Next(1, 8),
                    Category = random.Next(100) < 40 ? clothingCategory : booksCategory,
                    DeliveryStatus = random.Next(100) < 70 ? deliveredStatus : receivedStatus,
                    CreatedOn = createdDate,
                    LastModifiedOn = lastModified
                });
            }

            // - Early morning packages (6-8 AM)
            for (int i = 0; i < 4; i++)
            {
                var createdDate = baseDate.AddDays(random.Next(0, 30))
                    .AddHours(random.Next(6, 8))
                    .AddMinutes(random.Next(0, 60));
                
                var deliveryHours = random.Next(8, 48);
                var lastModified = createdDate.AddHours(deliveryHours);
                
                packages.Add(new Package
                {
                    UserId = testUsers[random.Next(testUsers.Count)].Id,
                    Height = random.Next(10, 20),
                    Width = random.Next(10, 20),
                    Depth = random.Next(7, 14),
                    PostBoxId = random.Next(1, 8),
                    Category = electronicsCategory,
                    DeliveryStatus = random.Next(100) < 60 ? inProgressStatus : deliveredStatus,
                    CreatedOn = createdDate,
                    LastModifiedOn = lastModified
                });
            }

            // - Late evening packages (20-22 PM)
            for (int i = 0; i < 4; i++)
            {
                var createdDate = baseDate.AddDays(random.Next(0, 30))
                    .AddHours(random.Next(20, 22))
                    .AddMinutes(random.Next(0, 60));
                
                var deliveryHours = random.Next(10, 60);
                var lastModified = createdDate.AddHours(deliveryHours);
                
                packages.Add(new Package
                {
                    UserId = testUsers[random.Next(testUsers.Count)].Id,
                    Height = random.Next(10, 20),
                    Width = random.Next(10, 20),
                    Depth = random.Next(7, 14),
                    PostBoxId = random.Next(1, 8),
                    Category = random.Next(100) < 50 ? clothingCategory : booksCategory,
                    DeliveryStatus = random.Next(100) < 50 ? pendingStatus : inProgressStatus,
                    CreatedOn = createdDate,
                    LastModifiedOn = lastModified
                });
            }

            // Ensure each user has at least a few packages for user activity analysis
            var userPackageCounts = new Dictionary<int, int>();
            foreach (var user in testUsers)
            {
                userPackageCounts[user.Id] = 0;
            }

            // Distribute packages more evenly among users
            foreach (var package in packages)
            {
                userPackageCounts[package.UserId]++;
            }

            // Add more packages for users with fewer packages
            var minPackagesPerUser = 8;
            foreach (var user in testUsers)
            {
                while (userPackageCounts[user.Id] < minPackagesPerUser)
                {
                    var createdDate = baseDate.AddDays(random.Next(0, 30))
                        .AddHours(random.Next(8, 20))
                        .AddMinutes(random.Next(0, 60));
                    
                    var deliveryHours = random.Next(4, 96);
                    var lastModified = createdDate.AddHours(deliveryHours);
                    
                    var status = random.Next(100) switch
                    {
                        < 20 => pendingStatus,
                        < 50 => inProgressStatus,
                        < 85 => deliveredStatus,
                        _ => receivedStatus
                    };

                    packages.Add(new Package
                    {
                        UserId = user.Id,
                        Height = random.Next(10, 30),
                        Width = random.Next(10, 30),
                        Depth = random.Next(7, 20),
                        PostBoxId = random.Next(1, 8),
                        Category = packageCategories[random.Next(packageCategories.Count)],
                        DeliveryStatus = status,
                        CreatedOn = createdDate,
                        LastModifiedOn = status == pendingStatus ? createdDate : lastModified
                    });
                    
                    userPackageCounts[user.Id]++;
                }
            }

            _context.Packages.AddRange(packages);
            await _context.SaveChangesAsync();
        }
    }
}

