namespace Neo4JConsolePackage_UnitTests
{
    using System;
    using System.Collections.Generic;
    using Anabranch.Neo4JConsolePackage;
    using FluentAssertions;
    using Xunit;

    public class Neo4jResponseTests
    {
        public class Constructor
        {
            [Fact]
            public void InitializesTheColumnsAndDataCollections()
            {
                var nr = new Neo4jResponse();
                nr.Columns.Should().NotBeNull();
                nr.Data.Should().NotBeNull();
            }
        }

        public class ToStringMethod
        {
            [Fact]
            public void ShowsNoDataMessage_WhenResponseIsEmpty()
            {
                const string expected =
@"+-------------------+
| No data returned. |
+-------------------+";

                var response = new Neo4jResponse();
                response.ToString().Should().Be(expected);
            }

            [Fact]
            public void ShowsNoDataResponse_WhenColumnsDefinedButNoDataReturned()
            {
                const string expected =
@"+-------------------+
| No data returned. |
+-------------------+";
                
                var response = new Neo4jResponse();
                response.Columns.Add("n");

                response.ToString().Should().Be(expected);
            }

            [Fact]
            public void ShowsSingleColumn_WhenDataExists()
            {
                const string expected =
@"+-------------+
| n           |
+-------------+
| {Foo:""Bar""} | 
+-------------+";

                var response = new Neo4jResponse();
                response.Columns.Add("n");
                response.Data.Add(new Neo4jData {Data = new Dictionary<string, string> {{"Foo", "Bar"}}});

                response.ToString().Should().Be(expected);
            }

            [Fact]
            public void ShowsSingleColumn_MultiRows_WhenDataExists()
            {
                const string expected =
@"+---------------+
| n             |
+---------------+
| {Foo:""Bar""}   | 
| {Foo2:""Bar2""} | 
+---------------+";

                var response = new Neo4jResponse();
                response.Columns.Add("n");
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo", "Bar" } } });
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo2", "Bar2" } } });

                response.ToString().Should().Be(expected);
            }


            [Fact] public void ColumnShouldBeAsWideAsTheLargestData()
            {
                const string expected =
@"+------------------------------------+
| n                                  |
+------------------------------------+
| {Foo:""Quite a long piece of data""} | 
| {Foo:""Bar""}                        | 
+------------------------------------+";

                var response = new Neo4jResponse();
                response.Columns.Add("n");
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo", "Quite a long piece of data" } } });
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo", "Bar" } } });

                response.ToString().Should().Be(expected);
            }

            [Fact]
            public void ShowsTwoColumns_WithData()
            {
                const string expected =
@"+---------------+---------------+
| n             | o             |
+---------------+---------------+
| {Foo:""Bar""}   | {Foo2:""Bar2""} | 
+---------------+---------------+";

                var response = new Neo4jResponse();
                response.Columns.Add("n");
                response.Columns.Add("o");
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo", "Bar" } } });
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo2", "Bar2" } } });

                response.ToString().Should().Be(expected); 
            }

            [Fact]
            public void ErrorsWhenTheDataDoesntFitInWithTheColumnCount()
            {
                const string expected = "ERROR: The data appears to be corrupt, we have 2 columns, but 1 bits of data.";
                var response = new Neo4jResponse();
                response.Columns.Add("n");
                response.Columns.Add("o");
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo", "Bar" } } });

                response.ToString().Should().Be(expected); 
            }
        }
    }
}