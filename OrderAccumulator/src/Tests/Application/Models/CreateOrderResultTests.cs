using Application.Models;
using FluentAssertions;

namespace Tests.Application.Models
{
    public class CreateOrderResultTests
    {
        [Fact]
        public void Accepted_ReturnsSuccessWithExposureAndNoError()
        {
            // Arrange
            var exposure = 32044.08m;

            // Act
            var result = CreateOrderResult.Accepted(exposure);

            // Assert
            result.Success.Should().BeTrue();
            result.CurrentExposure.Should().Be(exposure);
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void Rejected_ReturnsFailureWithExposureAndReason()
        {
            // Arrange
            var exposure = -500m;
            var reason = "Limite excedido.";

            // Act
            var result = CreateOrderResult.Rejected(exposure, reason);

            // Assert
            result.Success.Should().BeFalse();
            result.CurrentExposure.Should().Be(exposure);
            result.ErrorMessage.Should().Be(reason);
        }

        [Fact]
        public void FactoryMethods_MatchConstructor()
        {
            // Arrange
            var exposure = 10m;
            var reason = "x";

            // Act
            var accepted = CreateOrderResult.Accepted(exposure);
            var rejected = CreateOrderResult.Rejected(exposure, reason);

            // Assert
            accepted.Should().Be(new CreateOrderResult(true, exposure, null));
            rejected.Should().Be(new CreateOrderResult(false, exposure, reason));
        }

        [Fact]
        public void Equality_IsByValue()
        {
            // Arrange
            var exposure = 1m;
            var reason = "a";

            // Act
            var rejected = CreateOrderResult.Rejected(exposure, reason);
            var sameRejected = CreateOrderResult.Rejected(exposure, reason);
            var otherReason = CreateOrderResult.Rejected(exposure, "b");
            var accepted = CreateOrderResult.Accepted(exposure);
            var otherExposure = CreateOrderResult.Accepted(2m);

            // Assert
            rejected.Should().Be(sameRejected);
            rejected.Should().NotBe(otherReason);
            accepted.Should().NotBe(otherExposure);
        }
    }
}
