using AutoFixture;
using AwesomeAssertions;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.Alb;
using MinimalLambda.Options;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(BaseHttpResultExtensions))]
public class BaseHttpResultExtensionsTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void StatusCode_CreatesResultWithStatusCodeAndDefaults()
    {
        // Arrange
        var statusCode = 404;

        // Act
        var result = AlbResult.StatusCode(statusCode);

        // Assert
        result.StatusCode.Should().Be(statusCode);
        result.Headers.Should().NotBeNull();
        result.Headers.Should().BeEmpty();
        result.Body.Should().BeNull();
        result.IsBase64Encoded.Should().BeFalse();
    }

    [Fact]
    public void Text_CreatesResultWithTextPlainContentType()
    {
        // Arrange
        var statusCode = 200;
        var body = "Hello, World!";

        // Act
        var result = AlbResult.Text(statusCode, body);

        // Assert
        result.StatusCode.Should().Be(statusCode);
        result.Body.Should().Be(body);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("text/plain; charset=utf-8");
    }

    [Fact]
    public void Json_CreatesResultWithApplicationJsonContentType()
    {
        // Arrange
        var statusCode = 201;
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.Json(statusCode, payload);
        result.PackPayload(options);

        // Assert
        result.StatusCode.Should().Be(statusCode);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        result.Body.Should().NotBeNull();
        result.Body.Should().Contain(payload.Name);
        result.Body.Should().Contain(payload.Value.ToString());
    }

    [Fact]
    public void Customize_ModifiesPropertiesAndReturnsInstanceForChaining()
    {
        // Arrange
        var result = AlbResult.StatusCode(200);
        var customDescription = "Custom Status";
        var customHeader = "X-Custom-Header";

        // Act
        var customizedResult = result
            .Customize(r => r.StatusDescription = customDescription)
            .Customize(r => r.Headers[customHeader] = "CustomValue");

        // Assert
        customizedResult.Should().BeSameAs(result);
        result.StatusDescription.Should().Be(customDescription);
        result.Headers.Should().ContainKey(customHeader);
        result.Headers[customHeader].Should().Be("CustomValue");
    }

    private record TestPayload(string Name, int Value);
}
