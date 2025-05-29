using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using EduSync_Assessment.Controllers;
using EduSync_Assessment.Data;
using EduSync_Assessment.Models;
using EduSync_Assessment.DTO;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace EduSync_Assessment.Tests.Controllers
{
    [TestFixture]
    public class UserTablesControllerTests
    {
        private Mock<AppDbContext> _mockContext;
        private UserTablesController _controller;
        private Mock<DbSet<UserTable>> _mockSet;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>();
            _mockSet = new Mock<DbSet<UserTable>>();
            _controller = new UserTablesController(_mockContext.Object);
        }

        [Test]
        public async Task GetUserTables_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<UserTable>
            {
                new UserTable { UserId = Guid.NewGuid(), Name = "User1", Email = "user1@test.com", Role = "Student" },
                new UserTable { UserId = Guid.NewGuid(), Name = "User2", Email = "user2@test.com", Role = "Instructor" }
            };

            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.UserTables).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.GetUserTables();

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var returnedUsers = okResult?.Value as List<UserReadDto>;
            Assert.That(returnedUsers?.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetUserTable_WithValidId_ReturnsUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new UserTable
            {
                UserId = userId,
                Name = "Test User",
                Email = "test@test.com",
                Role = "Student"
            };

            _mockContext.Setup(c => c.UserTables.FindAsync(userId))
                       .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserTable(userId);

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var returnedUser = okResult?.Value as UserReadDto;
            Assert.That(returnedUser?.UserId, Is.EqualTo(userId));
            Assert.That(returnedUser?.Name, Is.EqualTo("Test User"));
        }

        [Test]
        public async Task PostUserTable_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new UserCreateDto
            {
                Name = "New User",
                Email = "newuser@test.com",
                Role = "Student",
                PasswordHash = "hashedpassword"
            };

            _mockContext.Setup(c => c.UserTables.Add(It.IsAny<UserTable>()));
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

            // Act
            var result = await _controller.PostUserTable(createDto);

            // Assert
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            var returnedUser = createdResult?.Value as UserReadDto;
            Assert.That(returnedUser?.Name, Is.EqualTo("New User"));
            Assert.That(returnedUser?.Email, Is.EqualTo("newuser@test.com"));
        }

        [Test]
        public async Task DeleteUserTable_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new UserTable { UserId = userId };

            _mockContext.Setup(c => c.UserTables.FindAsync(userId))
                       .ReturnsAsync(user);

            // Act
            var result = await _controller.DeleteUserTable(userId);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteUserTable_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockContext.Setup(c => c.UserTables.FindAsync(userId))
                       .ReturnsAsync((UserTable)null);

            // Act
            var result = await _controller.DeleteUserTable(userId);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }
    }
} 