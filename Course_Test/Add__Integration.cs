using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlazorApp3.Data;
using BlazorApp3.Models;
using BlazorApp3.Components.Pages.CoursesPages;
using Microsoft.AspNetCore.Components;

namespace Testing_SIMS2.Course_Test
{
    public class AddCourseIntegrationTests : TestContext
    {
        public AddCourseIntegrationTests()
        {
            // Setup real SQL Server database for testing using the connection string from the configuration class
            Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(DatabaseConfiguration.ConnectionString));
        }

        [Fact]
        public async Task Add_Course_Integration()
        {
            // Arrange
            var contextFactory = Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = contextFactory.CreateDbContext();

            // Start a transaction
            using var transaction = await context.Database.BeginTransactionAsync();

            // Prepare test data
            var testLecture = await context.ApplicationUser.FirstOrDefaultAsync();
            var testSemester = await context.Semesters.FirstOrDefaultAsync();
            var testSubject = await context.Subjects.FirstOrDefaultAsync();

            // Act
            var cut = RenderComponent<Create>();

            // Fill in the form
            await cut.InvokeAsync(() => {
                cut.Find("input#name").Change("Test Course");
                cut.Find("input#startdate").Change(DateTime.Now.ToString("yyyy-MM-dd"));
                cut.Find("input#enddate").Change(DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd"));
                cut.Find("select#semesterid").Change(testSemester.Id.ToString());
                cut.Find("select#lectureid").Change(testLecture.Id);
                cut.Find("select#subjectid").Change(testSubject.Id.ToString());
            });

            // Simulate form submission
            await cut.InvokeAsync(() => cut.Find("button[type='submit']").Click());

            // Assert
            await Task.Delay(1000); // Wait for async operations to complete

            var course = await context.Courses
                .Where(c => c.Name == "Test Course" &&
                            c.SemesterId == testSemester.Id &&
                            c.LectureId == testLecture.Id &&
                            c.SubjectId == testSubject.Id)
                .FirstOrDefaultAsync();

            Assert.NotNull(course); // Check if the course was added
            Assert.Equal("Test Course", course.Name);
            Assert.Equal(testSemester.Id, course.SemesterId);
            Assert.Equal(testLecture.Id, course.LectureId);
            Assert.Equal(testSubject.Id, course.SubjectId);

            // Rollback the transaction
            await transaction.RollbackAsync();
        }
    }
}
