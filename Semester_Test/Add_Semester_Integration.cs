using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlazorApp3.Data;
using BlazorApp3.Models;
using BlazorApp3.Components.Pages.SemestersPages;
using Microsoft.AspNetCore.Components;
namespace Testing_SIMS2
{
    public class AddSemesterIntegrationTests : TestContext
{
    public AddSemesterIntegrationTests()
    {
        // Setup real SQL Server database for testing using the connection string from the configuration class
        Services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlServer(DatabaseConfiguration.ConnectionString));
    }

    [Fact]
    public async Task Add_Semester_Integration()
    {
        // Arrange
        var contextFactory = Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var context = contextFactory.CreateDbContext();

        // Start a transaction
        using var transaction = await context.Database.BeginTransactionAsync();

        // Act
        var cut = RenderComponent<Create>();

        // Fill in the form
        cut.Find("input#name").Change("Fall 2024");
        cut.Find("input#startdate").Change("2024-09-01");
        cut.Find("input#enddate").Change("2024-12-15");

        // Simulate form submission
        cut.Find("button[type='submit']").Click();

        // Assert
        await Task.Delay(1000); // Wait for async operations to complete

        var semester = await context.Semesters
            .Where(s => s.Name == "Fall 2024" &&
                        s.StartDate == new DateTime(2024, 9, 1) &&
                        s.EndDate == new DateTime(2024, 12, 15))
            .FirstOrDefaultAsync();

        Assert.NotNull(semester); // Check if the semester was added
        Assert.Equal("Fall 2024", semester.Name);
        Assert.Equal(new DateTime(2024, 9, 1), semester.StartDate);
        Assert.Equal(new DateTime(2024, 12, 15), semester.EndDate);

        // Rollback the transaction
        await transaction.RollbackAsync();
    }
}
}