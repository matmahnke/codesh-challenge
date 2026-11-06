using Domain.Exceptions;
using FluentAssertions;

namespace Tests.UnitTests.Domain.Exceptions
{
    public class DomainExceptionTests
    {
        [Fact]
        public void Constructor_SetsMessage()
        {
            // Arrange
            var message = "Regra violada.";

            // Act
            var exception = new DomainException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.InnerException.Should().BeNull();
        }

        [Fact]
        public void IsAnException()
        {
            // Arrange
            var message = "x";

            // Act
            var exception = new DomainException(message);

            // Assert
            exception.Should().BeAssignableTo<Exception>();
        }

        [Fact]
        public void CanBeThrownAndCaughtWithMessage()
        {
            // Arrange
            var message = "Falha de domínio.";

            // Act
            Action act = () => throw new DomainException(message);

            // Assert
            act.Should().Throw<DomainException>().WithMessage(message);
        }
    }
}
