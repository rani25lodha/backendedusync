using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using EduSync_Assessment.Controllers;
using EduSync_Assessment.Data;
using EduSync_Assessment.Models;
using System.Collections.Generic;

namespace EduSync_Assessment.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<AppDbContext> _mockContext;
        private Mock<IConfiguration> _mockConfiguration;
        private AuthController _controller;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>();
            _mockConfiguration = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            
            mockConfigSection.Setup(s => s["Key"]).Returns("your-test-secret-key-min-16-chars");
            mockConfigSection.Setup(s => s["Issuer"]).Returns("test-issuer");
            mockConfigSection.Setup(s => s["Audience"]).Returns("test-audience");
            mockConfigSection.Setup(s => s["DurationInMinutes"]).Returns("60");

            _mockConfiguration.Setup(c => c.GetSection("Jwt")).Returns(mockConfigSection.Object);
            
            _controller = new AuthController(_mockContext.Object, _mockConfiguration.Object);
        }

        [Test]
        public void Login_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@test.com", Password = "password123" };
            var user = new UserTable 
            { 
                UserId = Guid.NewGuid(),
                Email = "test@test.com",
                PasswordHash = "password123",
                Name = "Test User",
                Role = "Student"
            };

            var users = new List<UserTable> { user }.AsQueryable();
            var mockSet = new Mock<DbSet<UserTable>>();
            mockSet.As<IQueryable<UserTable>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<UserTable>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<UserTable>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<UserTable>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            _mockContext.Setup(c => c.UserTables).Returns(mockSet.Object);

            // Act
            var result = _controller.Login(loginDto);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public void Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@test.com", Password = "wrongpassword" };
            var users = new List<UserTable>().AsQueryable();
            var mockSet = new Mock<DbSet<UserTable>>();
            mockSet.As<IQueryable<UserTable>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<UserTable>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<UserTable>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<UserTable>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            _mockContext.Setup(c => c.UserTables).Returns(mockSet.Object);

            // Act
            var result = _controller.Login(loginDto);

            // Assert
            Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
        }
    }
} 