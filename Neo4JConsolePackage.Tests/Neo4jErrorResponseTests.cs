namespace Neo4JConsolePackage_UnitTests
{
    using System;
    using Anabranch.Neo4JConsolePackage;
    using FluentAssertions;
    using Xunit;

    public class Neo4jErrorResponseTests
    {
        public class ToStringMethod
        {
            [Fact]
            public void ReplacesUnixLineEndingsWithEnvironmentEndings()
            {
                string expected = string.Format("Hello{0}World{0}", Environment.NewLine);
                var response = new Neo4jErrorResponse {Message = "Hello\nWorld"};

                response.ToString().Should().Be(expected);
            }
        }
    }
}