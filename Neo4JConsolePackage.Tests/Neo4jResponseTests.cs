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
            public void FormatsCorrectly_WhenContainingSimpleData()
            {
                var expected = string.Format("+----------+{0}| count(n) |{0}+----------+{0}| 2075     | {0}+----------+", Environment.NewLine);

                var response = new Neo4jResponse();
                response.Columns.Add("count(n)");
                response.Data.Add(new Neo4jSimpleData {Data = 2075});
                response.ToString().Should().Be(expected);
            }

            [Fact]
            public void ShowsNoDataMessage_WhenResponseIsEmpty()
            {
                var expected = string.Format(@"+-------------------+{0}| No data returned. |{0}+-------------------+", Environment.NewLine);

                var response = new Neo4jResponse();
                response.ToString().Should().Be(expected);
            }

            [Fact]
            public void ShowsNoDataResponse_WhenColumnsDefinedButNoDataReturned()
            {
                var expected = string.Format(@"+-------------------+{0}| No data returned. |{0}+-------------------+", Environment.NewLine);

                var response = new Neo4jResponse();
                response.Columns.Add("n");

                response.ToString().Should().Be(expected);
            }

            [Fact]
            public void ShowsSingleColumn_WhenDataExists()
            {
                string expected = string.Format(@"+-------------+{0}| n           |{0}+-------------+{0}| {{Foo:""Bar""}} | {0}+-------------+", Environment.NewLine);

                var response = new Neo4jResponse();
                response.Columns.Add("n");
                response.Data.Add(new Neo4jData {Data = new Dictionary<string, string> {{"Foo", "Bar"}}});

                response.ToString().Should().Be(expected);
            }

            [Fact]
            public void ShowsSingleColumn_MultiRows_WhenDataExists()
            {
                string expected =string.Format("+---------------+{0}| n             |{0}+---------------+{0}| {{Foo:\"Bar\"}}   | {0}| {{Foo2:\"Bar2\"}} | {0}+---------------+", Environment.NewLine);

                var response = new Neo4jResponse();
                response.Columns.Add("n");
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo", "Bar" } } });
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo2", "Bar2" } } });

                response.ToString().Should().Be(expected);
            }


            [Fact] public void ColumnShouldBeAsWideAsTheLargestData()
            {
                 string expected = string.Format("+------------------------------------+{0}| n                                  |{0}+------------------------------------+{0}| {{Foo:\"Quite a long piece of data\"}} | {0}| {{Foo:\"Bar\"}}                        | {0}+------------------------------------+", Environment.NewLine);

                var response = new Neo4jResponse();
                response.Columns.Add("n");
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo", "Quite a long piece of data" } } });
                response.Data.Add(new Neo4jData { Data = new Dictionary<string, string> { { "Foo", "Bar" } } });

                response.ToString().Should().Be(expected);
            }

            [Fact]
            public void ShowsTwoColumns_WithData()
            {
                string expected = "+---------------+---------------+" + Environment.NewLine + "| n             | o             |" + Environment.NewLine + "+---------------+---------------+" + Environment.NewLine + "| {Foo:\"Bar\"}   | {Foo2:\"Bar2\"} | " + Environment.NewLine + "+---------------+---------------+";

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