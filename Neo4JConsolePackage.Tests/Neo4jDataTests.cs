namespace Neo4JConsolePackage_UnitTests
{
    using Anabranch.Neo4JConsolePackage;
    using FluentAssertions;
    using Xunit;

    public class Neo4jDataTests
    {
        public class Constructor
        {
            [Fact]
            public void InitialisesTheDataDictionary()
            {
                var nd = new Neo4jData();
                nd.Data.Should().NotBeNull();
            }
        }

        public class ToStringMethod
        {
            [Fact]
            public void ReturnsEmptyResponse_WhenThereIsNoData()
            {
                var nd = new Neo4jData();
                nd.ToString().Should().Be("{}");
            }

            [Fact]
            public void ReturnsSingleResponse_WhenThereIsOnePieceOfData()
            {
                var nd = new Neo4jData();
                nd.Data.Add("Foo", "Bar");
                nd.ToString().Should().Be("{Foo:\"Bar\"}");
            }

            [Fact]
            public void ReturnsSingleResponse_WhenThereAreTwoPiecesOfData()
            {
                var nd = new Neo4jData();
                nd.Data.Add("Foo", "Bar");
                nd.Data.Add("Bar", "Foo");
                nd.ToString().Should().Be("{Foo:\"Bar\",Bar:\"Foo\"}");
            }
        }
    }
}