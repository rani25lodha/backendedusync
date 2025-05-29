using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using EduSync_Assessment.Controllers;
using EduSync_Assessment.Data;
using EduSync_Assessment.Models;
using EduSync_Assessment.DTO;
using EduSync_Assessment.BlobServices;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace EduSync_Assessment.Tests.Controllers
{
    [TestFixture]
    public class CourseTablesControllerTests
    {
        private Mock<AppDbContext> _mockContext;
        private Mock<IBlobStorageService> _mockBlobService;
        private CourseTablesController _controller;
        private Mock<DbSet<CourseTable>> _mockSet;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>();
            _mockBlobService = new Mock<IBlobStorageService>();
            _mockSet = new Mock<DbSet<CourseTable>>();
            _controller = new CourseTablesController(_mockContext.Object, _mockBlobService.Object);
        }

        [Test]
        public async Task GetCourseTable_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course = new CourseTable
            {
                CourseId = courseId,
                Title = "Test Course",
                Description = "Test Description",
                InstructorId = Guid.NewGuid(),
                MediaUrl = "test-url"
            };

            _mockContext.Setup(c => c.CourseTables.FindAsync(courseId))
                       .ReturnsAsync(course);

            // Act
            var result = await _controller.GetCourseTable(courseId);

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var returnedDto = okResult?.Value as CourseReadDto;
            Assert.That(returnedDto?.CourseId, Is.EqualTo(courseId));
            Assert.That(returnedDto?.Title, Is.EqualTo("Test Course"));
        }

        [Test]
        public async Task GetCourseTable_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            _mockContext.Setup(c => c.CourseTables.FindAsync(courseId))
                       .ReturnsAsync((CourseTable)null);

            // Act
            var result = await _controller.GetCourseTable(courseId);

            // Assert
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task PostCourseTable_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CourseCreateDto
            {
                Title = "New Course",
                Description = "New Description",
                InstructorId = Guid.NewGuid(),
                MediaUrl = "new-url"
            };

            _mockContext.Setup(c => c.CourseTables.Add(It.IsAny<CourseTable>()));
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

            // Act
            var result = await _controller.PostCourseTable(createDto);

            // Assert
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            var returnedDto = createdResult?.Value as CourseReadDto;
            Assert.That(returnedDto?.Title, Is.EqualTo("New Course"));
        }

        [Test]
        public async Task UploadCourseMedia_WithValidFile_ReturnsOkResult()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.jpg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

            var expectedUrl = "https://test-url/test.jpg";
            _mockBlobService.Setup(b => b.UploadFileAsync(It.IsAny<IFormFile>()))
                           .ReturnsAsync(expectedUrl);

            // Act
            var result = await _controller.UploadCourseMedia(fileMock.Object);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult?.Value, Is.Not.Null);
            
            // Convert the anonymous object to a dictionary for easier property access
            var resultDict = okResult.Value.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(okResult.Value));

            Assert.That(resultDict.ContainsKey("success"), Is.True);
            Assert.That(resultDict.ContainsKey("url"), Is.True);
            Assert.That(resultDict["success"], Is.EqualTo(true));
            Assert.That(resultDict["url"], Is.EqualTo(expectedUrl));
        }
    }
} 