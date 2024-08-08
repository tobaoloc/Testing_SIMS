using System.Threading.Tasks;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlazorApp3.Data;
using BlazorApp3.Models;
using BlazorApp3.Components.Pages.SemestersPages;

namespace Testing_SIMS2.Semester_Test
{
    public class Add_Semester_UnitTest : TestContext
    {
        [Fact]
        public async Task AddSemesters_ShouldAddSemesterToDatabase()
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
            component.Find("input#name").Change("Fall 2024");
            component.Find("input#startdate").Change("2024-09-01");
            component.Find("input#enddate").Change("2024-12-15");
            component.Find("button").Click();

            // Assert
            using var context = dbContextFactory.CreateDbContext();
            var savedSemester = await context.Semesters.FirstOrDefaultAsync(s => s.Name == "Fall 2024");
            Assert.NotNull(savedSemester);
            Assert.Equal("Fall 2024", savedSemester.Name);
            Assert.Equal(new DateTime(2024, 9, 1), savedSemester.StartDate);
            Assert.Equal(new DateTime(2024, 12, 15), savedSemester.EndDate);
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
