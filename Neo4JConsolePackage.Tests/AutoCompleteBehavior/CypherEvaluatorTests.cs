namespace Neo4JConsolePackage.Tests.AutoCompleteBehavior
{
    using Anabranch.Neo4JConsolePackage.AutoCompleteBehavior;
    using FluentAssertions;
    using Xunit;

    public class CypherEvaluatorTests
    {
        public class IsInRelationshipMethod
        {
            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData(null)]
            public void ReturnsFalse_WhenQueryIsNullEmptyOrWhitespace(string cypher)
            {
                CypherEvaluator.IsInARelationship(cypher).Should().BeFalse();
            }

            [Theory]
            [InlineData("-[id:Label]-")]
            [InlineData("]-")]
            [InlineData("MATCH (n:Label) RETURN x")]
            [InlineData("MATCH (n:Label)-[id:Label2\r\n")]
            public void ReturnsFalse_WhenQueryIsNotInANode(string cypher)
            {
                CypherEvaluator.IsInARelationship(cypher).Should().BeFalse();
            }

            [Theory]
            [InlineData("[")]
            [InlineData("-[:")]
            [InlineData("MATCH (n:Label)-[id:Label2")]
            public void ReturnsTrue_WhenQueryIsInANode(string cypher)
            {
                CypherEvaluator.IsInARelationship(cypher).Should().BeTrue();
            }
        }

        public class IsInNodeIdentifierMethod
        {
            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData(null)]
            public void ReturnsFalse_WhenQueryIsNullEmptyOrWhitespace(string cypher)
            {
                CypherEvaluator.IsInNodeIdentifier(cypher).Should().BeFalse();
            }

            [Theory]
            [InlineData("(n:Label)")]
            [InlineData(")")]
            [InlineData("MATCH (n:Label) RETURN x")]
            [InlineData("MATCH (n:Label\r\n")]
            public void ReturnsFalse_WhenQueryIsNotInARelationship(string cypher)
            {
                CypherEvaluator.IsInNodeIdentifier(cypher).Should().BeFalse();
            }

            [Theory]
            [InlineData("(")]
            [InlineData("-(:")]
            [InlineData("MATCH (n:Label")]
            public void ReturnsTrue_WhenQueryIsInARelationship(string cypher)
            {
                CypherEvaluator.IsInNodeIdentifier(cypher).Should().BeTrue();
            }
        }

        public class WasLastSignificantCharARelationshipLabelMethod
        {
            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData(null)]
            public void ReturnsFalse_WhenQueryIsNullEmptyOrWhitespace(string cypher)
            {
                CypherEvaluator.WasLastSignificantCharARelationshipLabel(cypher).Should().BeFalse();
            }

            [Theory]
            [InlineData("[")]
            [InlineData("-[id:Label]-")]
            [InlineData("]-")]
            [InlineData("MATCH (n:Label) RETURN x")]
            [InlineData("MATCH (n:Label)-[id:Label2\r\n")]
            public void ReturnsFalse_WhenQueryIsNotInARelationship(string cypher)
            {
                CypherEvaluator.WasLastSignificantCharARelationshipLabel(cypher).Should().BeFalse();
            }

            [Theory]
            [InlineData("-[:")]
            [InlineData("MATCH (n:Label)-[id:")]
            [InlineData("MATCH (n:Label)-[id:Label2")]
            [InlineData("MATCH (n:Label)-[id:Label2\r\n[:")]
            public void ReturnsTrue_WhenQueryIsInARelationship(string cypher)
            {
                CypherEvaluator.WasLastSignificantCharARelationshipLabel(cypher).Should().BeTrue();
            }
        }

        public class WasLastSignificantCharANodeLabelMethod
        {
            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData(null)]
            public void ReturnsFalse_WhenQueryIsNullEmptyOrWhitespace(string cypher)
            {
                CypherEvaluator.WasLastSignificantCharANodeLabel(cypher).Should().BeFalse();
            }

            [Theory]
            [InlineData("(")]
            [InlineData("-(id:Label)-")]
            [InlineData(")-")]
            [InlineData("MATCH (n:Label) RETURN x")]
            [InlineData("MATCH (n:Label\r\n")]
            public void ReturnsFalse_WhenQueryIsNotInANode(string cypher)
            {
                CypherEvaluator.WasLastSignificantCharANodeLabel(cypher).Should().BeFalse();
            }

            [Theory]
            [InlineData("-(:")]
            [InlineData("(:")]
            [InlineData("MATCH (n:")]
            [InlineData("MATCH (n:Label")]
            [InlineData("MATCH (n:Label)-[id:Label2\r\n(:")]
            public void ReturnsTrue_WhenQueryIsInANode(string cypher)
            {
                CypherEvaluator.WasLastSignificantCharANodeLabel(cypher).Should().BeTrue();
            }
        }
    }
}