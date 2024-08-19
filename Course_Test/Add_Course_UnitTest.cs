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

namespace YourApp.Tests
{
    public class AddCourse_UnitTest : TestContext
    {
        public AddCourse_UnitTest()
        {
            // Setup in-memory database for testing
            Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("UnitTestDatabase"));
        }

        [Fact]
        public async Task Add_Course_Unit()
        {
            // Arrange
            var contextFactory = Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var context = contextFactory.CreateDbContext();

            // Prepare test data
            var testLecture = new ApplicationUser
            {
                Id = "1",
                Name = "Nguyen Van A",
                Code = "LECTURE01", // Ensure all required properties are set
                Role = "Lecturer"   // Ensure all required properties are set
            };
            var testSemester = new Semesters
            {
                Id = 1,
                Name = "Spring",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 4, 1)
            };
            var testSubject = new Subjects
            {
                Id = 1,
                Name = "Database Design",
                Code = "SUBJ01" // Ensure all required properties are set
            };

            context.ApplicationUser.Add(testLecture);
            context.Semesters.Add(testSemester);
            context.Subjects.Add(testSubject);
            await context.SaveChangesAsync();

            // Render the component
            var cut = RenderComponent<Create>(); // Replace Create with the actual component name

            // Set initial values for the component's parameters if needed
            cut.Instance.Courses = new Courses
            {
                // Initialize with test data if needed
            };

            // Act
            // Fill in the form
            await cut.InvokeAsync(() => {
                cut.Find("input#name").Change("SPR001");
                cut.Find("input#startdate").Change("2024-01-01");
                cut.Find("input#enddate").Change("2024-04-01");
                cut.Find("select#semesterid").Change(testSemester.Id.ToString());
                cut.Find("select#lectureid").Change(testLecture.Id);
                cut.Find("select#subjectid").Change(testSubject.Id.ToString());
            });

            // Simulate form submission
            await cut.InvokeAsync(() => cut.Find("button[type='submit']").Click());

            // Assert
            var course = await context.Courses
                .Where(c => c.Name == "SPR001" &&
                            c.SemesterId == testSemester.Id &&
                            c.LectureId == testLecture.Id &&
                            c.SubjectId == testSubject.Id)
                .FirstOrDefaultAsync();

            Assert.NotNull(course); // Check if the course was added
            Assert.Equal("SPR001", course.Name);
            Assert.Equal(testSemester.Id, course.SemesterId);
            Assert.Equal(testLecture.Id, course.LectureId);
            Assert.Equal(testSubject.Id, course.SubjectId);
            Assert.Equal(new DateTime(2024, 1, 1), course.StartDate);
            Assert.Equal(new DateTime(2024, 4, 1), course.EndDate);
        }
    }
}
