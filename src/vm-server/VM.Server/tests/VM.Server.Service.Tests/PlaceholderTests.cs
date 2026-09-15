using FluentAssertions;

namespace VM.Server.Service.Tests;

public class PlaceholderTests
{
    [Fact]
    public void Scaffold_WhenProjectBuilds_AssertsTrue() => true.Should().BeTrue();
}
