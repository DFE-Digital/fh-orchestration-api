using FamilyHubs.Orchestration.Api.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FamilyHubs.Orchestration.Api.UnitTests;

#pragma warning disable CS1998

public class WhenUsingCorrelationMiddleware
{
    [Fact]
    public async Task ThenInvokeAsync_NoCorrelationIdHeader_GeneratesNewCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var loggerMock = new Mock<ILogger<CorrelationMiddleware>>();
        var middleware = new CorrelationMiddleware(loggerMock.Object);
        var nextDelegateCalled = false;

        context.Request.Headers["X-Correlation-ID"] = string.Empty;

        async Task NextDelegate(HttpContext innerContext)
        {
            nextDelegateCalled = true;
            // Assert that the correlation ID is set in the logger's scope.
            loggerMock.Verify(logger => logger.BeginScope(It.IsAny<Dictionary<string, object>>()), Times.Once);
            // Assert that the correlation ID in the scope matches the one generated.
            var correlationIdInScope = loggerMock.Invocations[0].Arguments[0] as Dictionary<string, object>;
            ArgumentNullException.ThrowIfNull(correlationIdInScope);
            correlationIdInScope["CorrelationId"].Should().NotBeNull();
        }

        // Act
        await middleware.InvokeAsync(context, NextDelegate);

        // Assert
        nextDelegateCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ThenInvokeAsync_CorrelationIdHeaderExists_UsesExistingCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var loggerMock = new Mock<ILogger<CorrelationMiddleware>>();
        var middleware = new CorrelationMiddleware(loggerMock.Object);
        var correlationId = Guid.NewGuid().ToString();

        context.Request.Headers["X-Correlation-ID"] = correlationId;

        async Task NextDelegate(HttpContext innerContext)
        {
            // Assert that the correlation ID is set in the logger's scope.
            loggerMock.Verify(logger => logger.BeginScope(It.IsAny<Dictionary<string, object>>()), Times.Once);
            // Assert that the correlation ID in the scope matches the one from the request.
            var correlationIdInScope = loggerMock.Invocations[0].Arguments[0] as Dictionary<string, object>;
            ArgumentNullException.ThrowIfNull(correlationIdInScope);
            correlationIdInScope["CorrelationId"].Should().Be(correlationId);
        }

        // Act
        await middleware.InvokeAsync(context, NextDelegate);
    }
}

#pragma warning restore CS1998
