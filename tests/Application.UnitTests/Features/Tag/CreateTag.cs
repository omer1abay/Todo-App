using Moq;
using Xunit;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.Tags.Commands.CreateTags;
using Entity = Todo_App.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Todo_App.Application.UnitTests.Features.Tags
{
    public class CreateTagsCommandTests
    {
        private Mock<IApplicationDbContext> _mockDbContext;
        private CreateTagsCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<IApplicationDbContext>();
            _handler = new CreateTagsCommandHandler(_mockDbContext.Object);
        }

        [Test]
        public async Task Handle_ShouldCreateTag_WhenValidRequest()
        {
            // Arrange
            var command = new CreateTagsCommand("New Tag");

            _mockDbContext.Setup(x => x.Tags.AddAsync(It.IsAny<Entity.Tags>(), It.IsAny<CancellationToken>())).Verifiable();
            _mockDbContext.Setup(x => x.Tags.AddAsync(It.IsAny<Entity.Tags>(), It.IsAny<CancellationToken>()))
                .Callback<Entity.Tags, CancellationToken>((entity, token) =>
                {
                    entity.Id = 1;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockDbContext.Verify(x => x.Tags.AddAsync(It.Is<Entity.Tags>(t => t.Name == command.name), It.IsAny<CancellationToken>()), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task Handle_ShouldThrowException_WhenNameIsNull()
        {
            // Arrange
            var command = new CreateTagsCommand(null);

            _mockDbContext.Setup(x => x.Tags.AddAsync(It.IsAny<Entity.Tags>(), It.IsAny<CancellationToken>())).Verifiable();
            _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}

