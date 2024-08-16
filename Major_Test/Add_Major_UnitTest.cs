using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlazorApp3.Data;
using BlazorApp3.Models;
using BlazorApp3.Components.Pages.MajorsPages;

namespace Testing_SIMS2.Major_Test
{
    public class Add_Major_UnitTest : TestContext
    {
        [Fact]
        public async Task AddMajor_ShouldAddMajorToDatabase()
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
            component.Find("select#departmentid").Change("1"); // Assuming department ID 1 exists
            component.Find("button").Click();

            // Assert
            using var context = dbContextFactory.CreateDbContext();
            var savedMajor = await context.Majors.FirstOrDefaultAsync(m => m.Name == "Computer Science");
            Assert.NotNull(savedMajor);
            Assert.Equal("Computer Science", savedMajor.Name);
            Assert.Equal(1, savedMajor.DepartmentId);
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
