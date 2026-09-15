using FluentAssertions;

namespace VM.Server.Domain.Tests;

public class PlaceholderTests
{
    [Fact]
    public void Scaffold_WhenProjectBuilds_AssertsTrue() => true.Should().BeTrue();
}
