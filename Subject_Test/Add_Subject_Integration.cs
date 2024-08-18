using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlazorApp3.Data;
using BlazorApp3.Models;
using BlazorApp3.Components.Pages.SubjectsPages;
using Microsoft.AspNetCore.Components;

namespace Testing_SIMS2.Subject_Test
{
    public class AddSubjectIntegrationTests : TestContext
    {
        public AddSubjectIntegrationTests()
        {
            // Setup real SQL Server database for testing using the connection string from the configuration class
            Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(DatabaseConfiguration.ConnectionString));
        }

        [Fact]
        public async Task Add_Subject_Integration()
        {
            // Arrange
            var contextFactory = Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = contextFactory.CreateDbContext();

            // Start a transaction
            using var transaction = await context.Database.BeginTransactionAsync();

            // Act
            var cut = RenderComponent<Create>(); // Ensure you have a CreateSubject component for adding subjects

            await cut.InvokeAsync(async () =>
            {
                // Fill in the form
                cut.Find("input#name").Change("Algorithms");
                cut.Find("select#majorid").Change("5"); // Assuming major ID 1 exists
                cut.Find("input#code").Change("CS101");

                // Simulate form submission
                cut.Find("button[type='submit']").Click();
            });

            // Assert
            await Task.Delay(1000); // Wait for async operations to complete

            var subject = await context.Subjects
                .Where(s => s.Name == "Algorithms" &&
                            s.MajorId == 5 &&
                            s.Code == "CS101")
                .FirstOrDefaultAsync();

            Assert.NotNull(subject); // Check if the subject was added
            Assert.Equal("Algorithms", subject.Name);
            Assert.Equal(5, subject.MajorId);
            Assert.Equal("CS101", subject.Code);

            // Rollback the transaction
            await transaction.RollbackAsync();
        }
    }
}
