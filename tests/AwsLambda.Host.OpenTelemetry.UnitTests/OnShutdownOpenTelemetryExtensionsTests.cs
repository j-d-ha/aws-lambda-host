using System.Diagnostics;
using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xunit;

namespace AwsLambda.Host.OpenTelemetry.UnitTests;

[TestSubject(typeof(OnShutdownOpenTelemetryExtensions))]
public class OnShutdownOpenTelemetryExtensionsTests
{
    #region OnShutdownFlushTracer Tests

    [Fact]
    public void OnShutdownFlushTracer_ThrowsOnNullILambdaOnShutdownBuilder()
    {
        // Arrange
        ILambdaOnShutdownBuilder mockApp = null!;

        // Act
        Action act = () => mockApp.OnShutdownFlushTracer();

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void OnShutdownFlushTracer_ThrowsOnNoTracerProviderRegistered()
    {
        // Arrange
        var mockApp = Substitute.For<ILambdaOnShutdownBuilder>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockServiceProvider.GetService(typeof(TracerProvider)).Returns(null);
        mockApp.Services.Returns(mockServiceProvider);

        // Act
        Action act = () => mockApp.OnShutdownFlushTracer();

        // Assert
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(
                "No service for type 'OpenTelemetry.Trace.TracerProvider' has been registered."
            );
    }

    [Fact]
    public void OnShutdownFlushTracer_ShouldNotThrowOnNoILoggerFactoryRegistered()
    {
        // Arrange
        var mockApp = Substitute.For<ILambdaOnShutdownBuilder>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockServiceProvider
            .GetService(typeof(TracerProvider))
            .Returns(Substitute.For<TracerProvider>());
        mockServiceProvider.GetService(typeof(ILoggerFactory)).Returns(null);
        mockApp.Services.Returns(mockServiceProvider);

        // Act
        Action act = () => mockApp.OnShutdownFlushTracer();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task OnShutdownFlushTracer_RegistersShutdownHandler_AndSuccessfullyFlushes()
    {
        // Arrange
        LambdaShutdownDelegate? capturedShutdownAction = null;
        var mockApp = CreateMockApp(a => capturedShutdownAction = a);

        var mockServiceProvider = Substitute.For<IServiceProvider>();

        // Act
        var result = mockApp.OnShutdownFlushTracer();

        capturedShutdownAction.Should().NotBeNull();
        await capturedShutdownAction.Invoke(mockServiceProvider, CancellationToken.None);

        // Assert
        result.Should().Be(mockApp);
        mockApp.ShutdownHandlers.Should().HaveCount(1);
        mockApp
            .Services.GetFakeLogCollector()
            .GetSnapshot()
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Information,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry tracer provider force flush succeeded",
                }
            );
    }

    [Fact]
    public async Task OnShutdownFlushTracer_RegistersShutdownHandler_AndFailedFlushes()
    {
        // Arrange
        LambdaShutdownDelegate? capturedShutdownAction = null;
        var options = new MockOptions { TracerShouldSucceed = false };
        var mockApp = CreateMockApp(a => capturedShutdownAction = a, options);

        var mockServiceProvider = Substitute.For<IServiceProvider>();

        // Act
        var result = mockApp.OnShutdownFlushTracer();

        capturedShutdownAction.Should().NotBeNull();
        await capturedShutdownAction.Invoke(mockServiceProvider, CancellationToken.None);

        // Assert
        result.Should().Be(mockApp);
        mockApp.ShutdownHandlers.Should().HaveCount(1);
        mockApp
            .Services.GetFakeLogCollector()
            .GetSnapshot()
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Information,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry tracer provider force flush failed",
                }
            );
    }

    [Fact]
    public async Task OnShutdownFlushTracer_RegistersShutdownHandler_AndCanceledFlushes()
    {
        // Arrange
        LambdaShutdownDelegate? capturedShutdownAction = null;
        var options = new MockOptions { TracerDelay = TimeSpan.FromMilliseconds(10) };
        var mockApp = CreateMockApp(a => capturedShutdownAction = a, options);

        var mockServiceProvider = Substitute.For<IServiceProvider>();

        // Act
        var result = mockApp.OnShutdownFlushTracer();

        capturedShutdownAction.Should().NotBeNull();
        await capturedShutdownAction.Invoke(mockServiceProvider, new CancellationToken(true));

        // Assert
        result.Should().Be(mockApp);
        mockApp.ShutdownHandlers.Should().HaveCount(1);
        mockApp
            .Services.GetFakeLogCollector()
            .GetSnapshot()
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Warning,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry tracer provider force flush failed to complete within allocated time",
                }
            );
    }

    #endregion

    #region OnShutdownFlushMeter Tests

    [Fact]
    public void OnShutdownFlushMeter_ThrowsOnNullILambdaOnShutdownBuilder()
    {
        // Arrange
        ILambdaOnShutdownBuilder mockApp = null!;

        // Act
        Action act = () => mockApp.OnShutdownFlushMeter();

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void OnShutdownFlushMeter_ThrowsOnNoMeterProviderRegistered()
    {
        // Arrange
        var mockApp = Substitute.For<ILambdaOnShutdownBuilder>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockServiceProvider.GetService(typeof(MeterProvider)).Returns(null);
        mockApp.Services.Returns(mockServiceProvider);

        // Act
        Action act = () => mockApp.OnShutdownFlushMeter();

        // Assert
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(
                "No service for type 'OpenTelemetry.Metrics.MeterProvider' has been registered."
            );
    }

    [Fact]
    public void OnShutdownFlushMeter_ShouldNotThrowOnNoILoggerFactoryRegistered()
    {
        // Arrange
        var mockApp = Substitute.For<ILambdaOnShutdownBuilder>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockServiceProvider
            .GetService(typeof(MeterProvider))
            .Returns(Substitute.For<MeterProvider>());
        mockServiceProvider.GetService(typeof(ILoggerFactory)).Returns(null);
        mockApp.Services.Returns(mockServiceProvider);

        // Act
        Action act = () => mockApp.OnShutdownFlushMeter();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task OnShutdownFlushMeter_RegistersShutdownHandler_AndSuccessfullyFlushes()
    {
        // Arrange
        LambdaShutdownDelegate? capturedShutdownAction = null;
        var mockApp = CreateMockApp(a => capturedShutdownAction = a);

        var mockServiceProvider = Substitute.For<IServiceProvider>();

        // Act
        var result = mockApp.OnShutdownFlushMeter();

        capturedShutdownAction.Should().NotBeNull();
        await capturedShutdownAction.Invoke(mockServiceProvider, CancellationToken.None);

        // Assert
        result.Should().Be(mockApp);
        mockApp.ShutdownHandlers.Should().HaveCount(1);
        mockApp
            .Services.GetFakeLogCollector()
            .GetSnapshot()
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Information,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry meter provider force flush succeeded",
                }
            );
    }

    [Fact]
    public async Task OnShutdownFlushMeter_RegistersShutdownHandler_AndFailedFlushes()
    {
        // Arrange
        LambdaShutdownDelegate? capturedShutdownAction = null;
        var options = new MockOptions { MeterShouldSucceed = false };
        var mockApp = CreateMockApp(a => capturedShutdownAction = a, options);

        var mockServiceProvider = Substitute.For<IServiceProvider>();

        // Act
        var result = mockApp.OnShutdownFlushMeter();

        capturedShutdownAction.Should().NotBeNull();
        await capturedShutdownAction.Invoke(mockServiceProvider, CancellationToken.None);

        // Assert
        result.Should().Be(mockApp);
        mockApp.ShutdownHandlers.Should().HaveCount(1);
        mockApp
            .Services.GetFakeLogCollector()
            .GetSnapshot()
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Information,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry meter provider force flush failed",
                }
            );
    }

    [Fact]
    public async Task OnShutdownFlushMeter_RegistersShutdownHandler_AndCanceledFlushes()
    {
        // Arrange
        LambdaShutdownDelegate? capturedShutdownAction = null;
        var options = new MockOptions { MeterDelay = TimeSpan.FromMilliseconds(10) };
        var mockApp = CreateMockApp(a => capturedShutdownAction = a, options);

        var mockServiceProvider = Substitute.For<IServiceProvider>();

        // Act
        var result = mockApp.OnShutdownFlushMeter();

        capturedShutdownAction.Should().NotBeNull();
        await capturedShutdownAction.Invoke(mockServiceProvider, new CancellationToken(true));

        // Assert
        result.Should().Be(mockApp);
        mockApp.ShutdownHandlers.Should().HaveCount(1);
        mockApp
            .Services.GetFakeLogCollector()
            .GetSnapshot()
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Warning,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry meter provider force flush failed to complete within allocated time",
                }
            );
    }

    #endregion

    #region OnShutdownFlushOpenTelemetry Tests

    [Fact]
    public void OnShutdownFlushOpenTelemetry_ThrowsOnNullILambdaOnShutdownBuilder()
    {
        // Arrange
        ILambdaOnShutdownBuilder mockApp = null!;

        // Act
        Action act = () => mockApp.OnShutdownFlushOpenTelemetry();

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void OnShutdownFlushOpenTelemetry_ReturnsApplicationForChaining()
    {
        // Arrange
        var mockApp = CreateMockApp(_ => { });

        // Act
        var result = mockApp.OnShutdownFlushOpenTelemetry();

        // Assert
        result.Should().Be(mockApp);
    }

    [Fact]
    public void OnShutdownFlushOpenTelemetry_RegistersBothFlushHandlers()
    {
        // Arrange
        var mockApp = CreateMockApp(_ => { });

        // Act
        mockApp.OnShutdownFlushOpenTelemetry();

        // Assert
        mockApp.ShutdownHandlers.Should().HaveCount(2);
    }

    [Fact]
    public void OnShutdownFlushOpenTelemetry_PassesTimeoutToHandlers()
    {
        // Arrange
        var mockApp = CreateMockApp(_ => { });
        const int customTimeout = 5000;

        // Act - Should not throw when passing custom timeout
        Action act = () => mockApp.OnShutdownFlushOpenTelemetry(customTimeout);

        // Assert
        act.Should().NotThrow();
        mockApp.ShutdownHandlers.Should().HaveCount(2);
    }

    [Fact]
    public async Task OnShutdownFlushOpenTelemetry_BothFlushesBothSucceed()
    {
        // Arrange
        var shutdownActions = new List<LambdaShutdownDelegate>();
        var mockApp = CreateMockApp(a => shutdownActions.Add(a));

        var mockServiceProvider = Substitute.For<IServiceProvider>();

        // Act
        var result = mockApp.OnShutdownFlushOpenTelemetry();

        shutdownActions.Should().HaveCount(2);
        await shutdownActions[0].Invoke(mockServiceProvider, CancellationToken.None);
        await shutdownActions[1].Invoke(mockServiceProvider, CancellationToken.None);

        // Assert
        result.Should().Be(mockApp);
        mockApp
            .Services.GetFakeLogCollector()
            .GetSnapshot()
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Information,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry meter provider force flush succeeded",
                }
            )
            .And.ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Information,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry tracer provider force flush succeeded",
                }
            );
    }

    [Fact]
    public async Task OnShutdownFlushOpenTelemetry_TracerFailsButMeterSucceeds()
    {
        // Arrange
        var shutdownActions = new List<LambdaShutdownDelegate>();
        var options = new MockOptions { TracerShouldSucceed = false };
        var mockApp = CreateMockApp(a => shutdownActions.Add(a), options);

        var mockServiceProvider = Substitute.For<IServiceProvider>();

        // Act
        var result = mockApp.OnShutdownFlushOpenTelemetry();

        shutdownActions.Should().HaveCount(2);
        await shutdownActions[0].Invoke(mockServiceProvider, CancellationToken.None);
        await shutdownActions[1].Invoke(mockServiceProvider, CancellationToken.None);

        // Assert
        result.Should().Be(mockApp);
        mockApp
            .Services.GetFakeLogCollector()
            .GetSnapshot()
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Information,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry meter provider force flush succeeded",
                }
            )
            .And.ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Information,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry tracer provider force flush failed",
                }
            );
    }

    [Fact]
    public async Task OnShutdownFlushOpenTelemetry_MeterFailsButTracerSucceeds()
    {
        // Arrange
        var shutdownActions = new List<LambdaShutdownDelegate>();
        var options = new MockOptions { MeterShouldSucceed = false };
        var mockApp = CreateMockApp(a => shutdownActions.Add(a), options);

        var mockServiceProvider = Substitute.For<IServiceProvider>();

        // Act
        var result = mockApp.OnShutdownFlushOpenTelemetry();

        shutdownActions.Should().HaveCount(2);
        await shutdownActions[0].Invoke(mockServiceProvider, CancellationToken.None);
        await shutdownActions[1].Invoke(mockServiceProvider, CancellationToken.None);

        // Assert
        result.Should().Be(mockApp);
        mockApp
            .Services.GetFakeLogCollector()
            .GetSnapshot()
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Information,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry meter provider force flush failed",
                }
            )
            .And.ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Information,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry tracer provider force flush succeeded",
                }
            );
    }

    [Fact]
    public async Task OnShutdownFlushOpenTelemetry_BothFlushesTimeout()
    {
        // Arrange
        var shutdownActions = new List<LambdaShutdownDelegate>();
        var options = new MockOptions
        {
            MeterDelay = TimeSpan.FromMilliseconds(10),
            TracerDelay = TimeSpan.FromMilliseconds(10),
        };
        var mockApp = CreateMockApp(a => shutdownActions.Add(a), options);

        var mockServiceProvider = Substitute.For<IServiceProvider>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = mockApp.OnShutdownFlushOpenTelemetry();

        shutdownActions.Should().HaveCount(2);
        await shutdownActions[0].Invoke(mockServiceProvider, cancellationToken);
        await shutdownActions[1].Invoke(mockServiceProvider, cancellationToken);

        // Assert
        result.Should().Be(mockApp);
        mockApp
            .Services.GetFakeLogCollector()
            .GetSnapshot()
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Warning,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry meter provider force flush failed to complete within allocated time",
                }
            )
            .And.ContainEquivalentOf(
                new
                {
                    Level = LogLevel.Warning,
                    Category = "AwsLambda.Host.OpenTelemetry",
                    Message = "OpenTelemetry tracer provider force flush failed to complete within allocated time",
                }
            );
    }

    #endregion

    #region Test Helpers


    private static ILambdaOnShutdownBuilder CreateMockApp(
        Action<LambdaShutdownDelegate> onShutdown,
        MockOptions? options = null
    )
    {
        options ??= new MockOptions();

        var mockApp = Substitute.For<ILambdaOnShutdownBuilder>();

        var actualList = new List<LambdaShutdownDelegate>();
        var shutdownHandlersMock = Substitute.For<IList<LambdaShutdownDelegate>>();

        // When Add is called, invoke the callback and add to the real list
        shutdownHandlersMock
            .When(x => x.Add(Arg.Any<LambdaShutdownDelegate>()))
            .Do(ci =>
            {
                var handler = (LambdaShutdownDelegate)ci[0];
                actualList.Add(handler);
                onShutdown(handler);
            });

        // Return the actual list count
        shutdownHandlersMock.Count.Returns(_ => actualList.Count);

        mockApp.ShutdownHandlers.Returns(shutdownHandlersMock);

        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder.AddProcessor(
                    new MockProcessor<Activity>(options.TracerDelay, options.TracerShouldSucceed)
                );
            })
            .WithMetrics(builder =>
            {
                builder.AddReader(
                    new MockMetricReader(options.MeterDelay, options.MeterShouldSucceed)
                );
            });

        serviceCollection.AddLogging(builder =>
        {
            builder.AddFakeLogging();
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();

        mockApp.Services.Returns(serviceProvider);

        return mockApp;
    }

    private class MockOptions
    {
        public TimeSpan TracerDelay { get; set; } = TimeSpan.Zero;
        public bool TracerShouldSucceed { get; set; } = true;
        public TimeSpan MeterDelay { get; set; } = TimeSpan.Zero;
        public bool MeterShouldSucceed { get; set; } = true;
    }

    private class MockProcessor<T>(TimeSpan delay, bool shouldSucceed) : BaseProcessor<T>
    {
        protected override bool OnForceFlush(int timeoutMilliseconds)
        {
            Thread.Sleep(delay);
            return shouldSucceed;
        }
    }

    private class MockMetricReader(TimeSpan delay, bool shouldSucceed) : MetricReader
    {
        protected override bool OnCollect(int timeoutMilliseconds)
        {
            Thread.Sleep(delay);
            return shouldSucceed;
        }
    }

    #endregion
}
