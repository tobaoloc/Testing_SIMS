using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlazorApp3.Data;
using BlazorApp3.Models;
using BlazorApp3.Components.Pages.MajorsPages;
using Microsoft.AspNetCore.Components;
namespace Testing_SIMS2.Major_Test
{
    public class AddMajorIntegrationTests : TestContext
    {
        public AddMajorIntegrationTests()
        {
            // Setup real SQL Server database for testing using the connection string from the configuration class
            Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(DatabaseConfiguration.ConnectionString));
        }

        [Fact]
        public async Task Add_Major_Integration()
        {
            // Arrange
            var contextFactory = Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = contextFactory.CreateDbContext();

            // Start a transaction
            using var transaction = await context.Database.BeginTransactionAsync();

            // Act
            var cut = RenderComponent<Create>();

            // Fill in the form
            cut.Find("input#name").Change("Computer Science");
            cut.Find("select#departmentid").Change("1"); // Assuming department ID 1 exists

            // Simulate form submission
            cut.Find("button[type='submit']").Click();

            // Assert
            await Task.Delay(1000); // Wait for async operations to complete

            var major = await context.Majors
                .Where(m => m.Name == "Computer Science" &&
                            m.DepartmentId == 1)
                .FirstOrDefaultAsync();

            Assert.NotNull(major); // Check if the major was added
            Assert.Equal("Computer Science", major.Name);
            Assert.Equal(1, major.DepartmentId);

            // Rollback the transaction
            await transaction.RollbackAsync();
        }
    }

}