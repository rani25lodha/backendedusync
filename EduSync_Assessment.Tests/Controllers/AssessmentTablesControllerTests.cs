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
    public class AssessmentTablesControllerTests
    {
        private Mock<AppDbContext> _mockContext;
        private AssessmentTablesController _controller;
        private Mock<DbSet<AssessmentTable>> _mockSet;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>();
            _mockSet = new Mock<DbSet<AssessmentTable>>();
            _controller = new AssessmentTablesController(_mockContext.Object);
        }

        [Test]
        public async Task GetAssessmentTables_ReturnsAllAssessments()
        {
            // Arrange
            var assessments = new List<AssessmentTable>
            {
                new AssessmentTable 
                { 
                    AssessmentId = Guid.NewGuid(),
                    Title = "Assessment 1",
                    Questions = "Question 1",
                    MaxScore = 100,
                    CourseId = Guid.NewGuid()
                },
                new AssessmentTable 
                { 
                    AssessmentId = Guid.NewGuid(),
                    Title = "Assessment 2",
                    Questions = "Question 2",
                    MaxScore = 50,
                    CourseId = Guid.NewGuid()
                }
            };

            var mockDbSet = assessments.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.AssessmentTables).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.GetAssessmentTables();

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var returnedAssessments = okResult?.Value as IEnumerable<AssessmentReadDto>;
            Assert.That(returnedAssessments?.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAssessmentTable_WithValidId_ReturnsAssessment()
        {
            // Arrange
            var assessmentId = Guid.NewGuid();
            var assessment = new AssessmentTable
            {
                AssessmentId = assessmentId,
                Title = "Test Assessment",
                Questions = "Test Questions",
                MaxScore = 100,
                CourseId = Guid.NewGuid()
            };

            _mockContext.Setup(c => c.AssessmentTables.FindAsync(assessmentId))
                       .ReturnsAsync(assessment);

            // Act
            var result = await _controller.GetAssessmentTable(assessmentId);

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var returnedAssessment = okResult?.Value as AssessmentReadDto;
            Assert.That(returnedAssessment?.AssessmentId, Is.EqualTo(assessmentId));
            Assert.That(returnedAssessment?.Title, Is.EqualTo("Test Assessment"));
        }

        [Test]
        public async Task GetAssessmentTable_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var assessmentId = Guid.NewGuid();
            _mockContext.Setup(c => c.AssessmentTables.FindAsync(assessmentId))
                       .ReturnsAsync((AssessmentTable)null);

            // Act
            var result = await _controller.GetAssessmentTable(assessmentId);

            // Assert
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task PostAssessmentTable_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new AssessmentCreateDto
            {
                Title = "New Assessment",
                Questions = "New Questions",
                MaxScore = 100,
                CourseId = Guid.NewGuid()
            };

            _mockContext.Setup(c => c.AssessmentTables.Add(It.IsAny<AssessmentTable>()));
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

            // Act
            var result = await _controller.PostAssessmentTable(createDto);

            // Assert
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            var returnedAssessment = createdResult?.Value as AssessmentReadDto;
            Assert.That(returnedAssessment?.Title, Is.EqualTo("New Assessment"));
            Assert.That(returnedAssessment?.MaxScore, Is.EqualTo(100));
        }
    }

    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> BuildMockDbSet<T>() where T : class
        {
            return new Mock<DbSet<T>>();
        }

        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> source) where T : class
        {
            var mock = new Mock<DbSet<T>>();
            mock.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(CancellationToken.None))
                .Returns(new TestAsyncEnumerator<T>(source.GetEnumerator()));

            mock.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(source.Provider));

            mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
            mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
            mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(source.GetEnumerator());

            return mock;
        }
    }

    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            return Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }
    }

    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }
} 