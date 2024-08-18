using System.Threading.Tasks;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlazorApp3.Data;
using BlazorApp3.Models;
using BlazorApp3.Components.Pages.DepartmentsPages;

namespace Testing_SIMS2.Department_Test
{
    public class Add_Department_UnitTest : TestContext
    {
        [Fact]
        public async Task AddDepartment_ShouldAddDepartmentToDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            var dbContextFactory = new TestDbContextFactory(options);

            // Setup bUnit component
            Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(dbContextFactory);
            var component = RenderComponent<Create>();

            // Act
            component.Find("input#name").Change("Computer Science");
            component.Find("button").Click();

            // Assert
            using var context = dbContextFactory.CreateDbContext();
            var savedDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Computer Science");
            Assert.NotNull(savedDepartment);
            Assert.Equal("Computer Science", savedDepartment.Name);
        }
    }

    // Helper class to create DbContextFactory
    public class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
        {
            _options = options;
        }

        public ApplicationDbContext CreateDbContext()
        {
            return new ApplicationDbContext(_options);
        }
    }
}
