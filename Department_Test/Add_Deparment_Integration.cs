using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlazorApp3.Data;
using BlazorApp3.Models;
using BlazorApp3.Components.Pages.DepartmentsPages;
using Microsoft.AspNetCore.Components;

namespace Testing_SIMS2
{
    public class AddDepartmentIntegrationTests : TestContext
    {
        public AddDepartmentIntegrationTests()
        {
            // Setup real SQL Server database for testing using the connection string from the configuration class
            Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(DatabaseConfiguration.ConnectionString));
        }

        [Fact]
        public async Task Add_Department_Integration()
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

            // Simulate form submission
            cut.Find("button[type='submit']").Click();

            // Assert
            await Task.Delay(1000); // Wait for async operations to complete

            var department = await context.Departments
                .Where(d => d.Name == "Computer Science")
                .FirstOrDefaultAsync();

            Assert.NotNull(department); // Check if the department was added
            Assert.Equal("Computer Science", department.Name);

            // Rollback the transaction
            await transaction.RollbackAsync();
        }
    }
}
