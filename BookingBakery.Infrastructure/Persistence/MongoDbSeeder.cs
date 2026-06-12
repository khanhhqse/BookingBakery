using BookingBakery.Domain.Models;
using MongoDB.Driver;

namespace BookingBakery.Infrastructure.Persistence
{
    public static class MongoDbSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var dbContext = (MongoDbContext?)serviceProvider.GetService(typeof(MongoDbContext))
                ?? throw new InvalidOperationException("MongoDbContext is not registered.");
            var roleCollection = dbContext.GetCollection<Role>("roles");

            var rolesToSeed = new List<Role>
            {
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Staff" },
                new Role { RoleId = 3, RoleName = "Customer" }
            };

            foreach (var role in rolesToSeed)
            {
                var filter = Builders<Role>.Filter.Eq(r => r.RoleId, role.RoleId);
                var existingRole = await roleCollection.Find(filter).FirstOrDefaultAsync();

                if (existingRole == null)
                {
                    await roleCollection.InsertOneAsync(role);
                }
                else if (existingRole.RoleName != role.RoleName)
                {
                    var update = Builders<Role>.Update.Set(r => r.RoleName, role.RoleName);
                    await roleCollection.UpdateOneAsync(filter, update);
                }
            }
        }
    }
}
