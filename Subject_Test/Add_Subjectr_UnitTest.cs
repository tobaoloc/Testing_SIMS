using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlazorApp3.Data;
using BlazorApp3.Models;
using BlazorApp3.Components.Pages.SubjectsPages;

namespace Testing_SIMS2.Subject_Test
{
    public class Add_Subjects_UnitTest : TestContext
    {
        [Fact]
        public async Task AddSubject_ShouldAddSubjectToDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            var dbContextFactory = new TestDbContextFactory(options);

            // Setup bUnit component
            Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(dbContextFactory);
            var component = RenderComponent<Create>();  // Assuming your component is named 'Create'

            // Act
            // Thêm một Major trước khi thêm Subject
            using (var context = dbContextFactory.CreateDbContext())
            {
                context.Majors.Add(new Majors { Name = "Computer Science", DepartmentId = 1 });
                await context.SaveChangesAsync();
            }

            // Thực hiện thêm Subject
            component.Find("input#name").Change("Data Structures");
            component.Find("select#majorid").Change("1"); // Assuming Major ID 1 exists
            component.Find("input#code").Change("CS101");
            component.Find("button").Click();

            // Assert
            using var dbContext = dbContextFactory.CreateDbContext();
            var savedSubject = await dbContext.Subjects.FirstOrDefaultAsync(s => s.Name == "Data Structures");
            Assert.NotNull(savedSubject);
            Assert.Equal("Data Structures", savedSubject.Name);
            Assert.Equal(1, savedSubject.MajorId);
            Assert.Equal("CS101", savedSubject.Code);
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
