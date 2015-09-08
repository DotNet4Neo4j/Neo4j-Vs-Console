namespace Neo4JConsolePackage.Tests
{
    using System;
    using System.Linq;
    using System.Windows;
    using Anabranch.Neo4JConsolePackage;
    using FluentAssertions;
    using Moq;
    using RestSharp;
    using Xunit;
    using Xunit.Extensions;

    public class Neo4jConsoleControlViewModelTests
    {
        private static IRestClient GetMockRestClient(string response = "{ \"columns\" : [ ], \"data\" : [ ] }")
        {
            var rr = new Mock<IRestResponse>();
            rr.Setup(r => r.Content).Returns(response);

            var rc = new Mock<IRestClient>();
            rc.Setup(r => r.Execute(It.IsAny<IRestRequest>())).Returns(rr.Object);

            return rc.Object;
        }

        public class ClearCommand
        {
            [Fact]
            public void CanAlwaysExecuteAndClearsTheCypherResults()
            {
                var vm = new Neo4jConsoleControlViewModel {CypherResults = "Blah"};

                vm.ClearCommand.CanExecute(null).Should().BeTrue();
                vm.ClearCommand.Execute(null);

                vm.CypherResults.Should().BeEmpty();
            }
        }

        public class NextHistoryCommand
        {
            [Fact]
            public void ShouldNotChangeTheCypherQuery_WhenUsedOnEmptyHistory()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Should().BeEmpty();
                vm.CypherQuery = "Foo";

                vm.NextHistoryCommand.Execute(null);
                vm.CypherQuery.Should().Be("Foo");
            }

            [Fact]
            public void ShouldSelectNext_WhenThereIsAQueryInTheHistory()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Add("Cypher 1");
                vm.CypherHistory.Should().HaveCount(1);

                vm.CypherQuery = "Foo";
                vm.NextHistoryCommand.Execute(null);
                vm.CypherQuery.Should().Be("Cypher 1");
            }
            
            [Fact]
            public void ShouldSelectNext_WhenThereIsAQueryInTheHistory_AndItsCalledTwice()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Add("Cypher 1");
                vm.CypherHistory.Should().HaveCount(1);

                vm.CypherQuery = "Foo";
                vm.NextHistoryCommand.Execute(null);
                vm.NextHistoryCommand.Execute(null);
                vm.CypherQuery.Should().Be("Cypher 1");
            }

            [Fact]
            public void ShouldSelectNextThenNext_WhenThereIsAQueryInTheHistory_AndItsCalledTwice()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Add("Cypher 1");
                vm.CypherHistory.Add("Cypher 2");
                vm.CypherHistory.Should().HaveCount(2);

                vm.CypherQuery = "Foo";
                vm.NextHistoryCommand.Execute(null);
                vm.NextHistoryCommand.Execute(null);
                vm.CypherQuery.Should().Be("Cypher 2");
            }
        }

        public class PreviousHistoryCommand
        {
            [Fact]
            public void ShouldNotChangeTheCypherQuery_WhenUsedOnEmptyHistory()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Should().BeEmpty();
                vm.CypherQuery = "Foo";

                vm.PreviousHistoryCommand.Execute(null);
                vm.CypherQuery.Should().Be("Foo");
            }

            [Fact]
            public void ShouldReturnAnEmptyString_RegardlessOfHowManyTimesItsCalled()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Add("Foo");
                
                vm.NextHistoryCommand.Execute(null);
                vm.CypherQuery.Should().Be("Foo");

                vm.PreviousHistoryCommand.Execute(null);
                vm.PreviousHistoryCommand.Execute(null);
                vm.CypherQuery.Should().Be("");
            }

            [Fact]
            public void ShouldNavigateBackwards_WhenNextHistoryHasBeenUsed()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Add("Foo");
                vm.CypherHistory.Add("Bar");

                vm.NextHistoryCommand.Execute(null);
                vm.CypherQuery.Should().Be("Foo");

                vm.NextHistoryCommand.Execute(null);
                vm.CypherQuery.Should().Be("Bar");

                vm.PreviousHistoryCommand.Execute(null);
                vm.CypherQuery.Should().Be("Foo");
            }
        }
        
        public class PostCommand
        {
            [Fact]
            public void ReturnsExceptionMessage_WhenPostingThrowsAnException()
            {
                const string expectedMessage = "Communicating to neo4j server threw a System.ArgumentException with this message: Exception Message";
                var rc = new Mock<IRestClient>();
                rc
                    .Setup(r => r.Execute(It.IsAny<IRestRequest>()))
                    .Throws(new ArgumentException("Exception Message"));

                var vm = new Neo4jConsoleControlViewModel(rc.Object);
                vm.PostCommand.Execute(null);

                vm.CypherResults.Should().Contain(expectedMessage);
            }

            [Theory,
             InlineData(null),
             InlineData(""),
             InlineData(" ")]
            public void ReturnsAndDoesNothing_WhenCypherQueryIsNullOrWhiteSpace(string cypherQuery)
            {
                var rc = new Mock<IRestClient>();
                var vm = new Neo4jConsoleControlViewModel(rc.Object) {CypherQuery = cypherQuery};

                rc.ResetCalls();
                vm.PostCommand.Execute(null);
                rc.Verify(r => r.Execute(It.IsAny<IRestRequest>()), Times.Never);
            }

            [Theory,
             InlineData(null),
             InlineData(""),
             InlineData(" ")]
            public void ReturnsAndDoesNothing_WhenNeo4jUrlIsNullOrWhiteSpace(string neo4jUrl)
            {
                var rc = new Mock<IRestClient>();
                var vm = new Neo4jConsoleControlViewModel(rc.Object) {Neo4jUrl = neo4jUrl};

                rc.ResetCalls();
                vm.PostCommand.Execute(null);
                rc.Verify(r => r.Execute(It.IsAny<IRestRequest>()), Times.Never);
            }

            [Fact]
            public void GivesNoResponseMessage_WhenResponseFromTheServerIsNullOrWhiteSpace()
            {
                const string expected = "No response from the server (THEURL), is it running?";
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient(null)) { Neo4jUrl = "THEURL" };

                vm.PostCommand.Execute(null);
                vm.CypherResults.Should().Contain(expected);
            }

            [Fact]
            public void GivesValidResponse_WhenJsonCanBeDeserializedIntoNeo4jResponse()
            {
                const string successWithData = "{ \"columns\" : [ \"n\" ], \"data\" : [ { \"data\" : { \"value\" : \"neo4jConsole\" } } ]}";
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient(successWithData));

                vm.PostCommand.Execute(null);
                vm.CypherResults.Should().Contain("{value:\"neo4jConsole\"}");
            }

            [Fact]
            public void GivesValidResponse_WhenJsonCanBeDeserializedIntoNeo4jResponseButHasNoData()
            {
                const string successWithNoData = "{ \"columns\" : [ ], \"data\" : [ ] }";
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient(successWithNoData));

                vm.PostCommand.Execute(null);
                vm.CypherResults.Should().Contain("No data returned.");
            }

            [Fact]
            public void GivesErrorResponse_WhenJsonCantBeDeserializedIntoNeo4jResponseButCanBeNeo4jErrorResponse()
            {
                const string errorResponse = "{ \"message\" : \"errorNeo4jConsole\", \"exception\" : \"AnException\", \"fullname\" : \"oops.AnException\", \"stacktrace\" : [ \"CodeLines (999)\"] }";
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient(errorResponse));

                vm.PostCommand.Execute(null);
                vm.CypherResults.Should().Contain("errorNeo4jConsole");
            }

            [Fact]
            public void GivesDefaultErrorMessageIfCantBeDeserializedIntoEitherNeo4jResponseOrNeo4jErrorResponse()
            {
                const string response = "non deserializable";
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient(response));

                vm.PostCommand.Execute(null);
                vm.CypherResults.Should().Contain("Couldn't deserialize");
                vm.CypherResults.Should().Contain(response);
            }

            [Fact]
            public void CallsBackOnTheCallbackMethod_WhenFinishing()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());

                bool calledBack = false;
                Action callback = () => calledBack = true;

                vm.PostCommand.Execute(callback);
                calledBack.Should().BeTrue();
            }

            [Fact]
            public void InsertsCypherQueryIntoHistory_AtFront_WhenNoHistoryPresent()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Count.Should().Be(0);
                vm.CypherQuery = "Foo";

                vm.PostCommand.Execute(null);
                vm.CypherHistory.Count.Should().Be(1);
                vm.CypherHistory.First().Should().Be("Foo");
            }

            [Fact]
            public void InsertsCypherQueryIntoHistory_AtNextPosition()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Add("Bar");
                vm.CypherHistory.Count.Should().Be(1);
                
                vm.NextHistoryCommand.Execute(null);
                vm.CypherQuery = "Foo";
                
                vm.PostCommand.Execute(null);
                vm.CypherHistory.Count.Should().Be(2);
                vm.CypherHistory.First().Should().Be("Bar");
                vm.CypherHistory.Skip(1).First().Should().Be("Foo");
            }

            [Fact]
            public void InsertsCypherQueryIntoHistory_AtCorrectPosition()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Add("Bar");
                vm.CypherHistory.Count.Should().Be(1);

                vm.CypherQuery = "Foo";

                vm.PostCommand.Execute(null);
                vm.CypherHistory.Count.Should().Be(2);
                vm.CypherHistory.First().Should().Be("Foo");
                vm.CypherHistory.Skip(1).First().Should().Be("Bar");
            }

            [Fact]
            public void InsertsCypherQueryIntoHistory_AtCorrectPosition2()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Add("Bar");
                vm.CypherHistory.Add("Spam");
                vm.CypherHistory.Count.Should().Be(2);

                vm.NextHistoryCommand.Execute(null);

                vm.CypherQuery = "Foo";

                vm.PostCommand.Execute(null);
                vm.CypherHistory.Count.Should().Be(3);
                vm.CypherHistory.First().Should().Be("Bar");
                vm.CypherHistory.Skip(1).First().Should().Be("Foo");
                vm.CypherHistory.Skip(2).First().Should().Be("Spam");
            }

            [Fact]
            public void ShouldntInsertDuplicateQueriesIntoHistory()
            {
                var vm = new Neo4jConsoleControlViewModel(GetMockRestClient());
                vm.CypherHistory.Add("Bar");
                vm.CypherHistory.Add("Spam");
                vm.CypherHistory.Count.Should().Be(2);

                vm.NextHistoryCommand.Execute(null);

                vm.CypherQuery = "Spam";

                vm.PostCommand.Execute(null);
                vm.CypherHistory.Count.Should().Be(2);
                vm.CypherHistory.First().Should().Be("Bar");
                vm.CypherHistory.Skip(1).First().Should().Be("Spam");
            }

        }

        public class ChangeWrappingCommand
        {
            [Fact]
            public void SwitchesToNoWrap_WhenResultsWrappingIsSetToWrap()
            {
                var vm = new Neo4jConsoleControlViewModel {ResultsWrapping = TextWrapping.Wrap};

                vm.ChangeWrappingCommand.Execute(null);
                vm.ResultsWrapping.Should().Be(TextWrapping.NoWrap);
            }

            [Fact]
            public void SwitchesToWrap_WhenResultsWrappingIsSetToNoWrap()
            {
                var vm = new Neo4jConsoleControlViewModel { ResultsWrapping = TextWrapping.NoWrap };

                vm.ChangeWrappingCommand.Execute(null);
                vm.ResultsWrapping.Should().Be(TextWrapping.Wrap);
            }
        }
    }
}
