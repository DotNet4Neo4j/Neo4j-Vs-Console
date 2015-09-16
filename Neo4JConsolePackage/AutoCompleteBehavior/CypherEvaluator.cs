namespace Anabranch.Neo4JConsolePackage.AutoCompleteBehavior
{
    using System;
    using System.Linq;

    /// <summary>A class providing helpers to evaluate the given Cypher</summary>
    public static class CypherEvaluator
    {
        #region Is In
        public static bool IsInNodeIdentifier(string cypher)
        {
            return IsInSomething(cypher, '(', ')');
        }

        public static bool IsInARelationship(string cypher)
        {
            return IsInSomething(cypher, '[', ']');
        }

        private static bool IsInSomething(string cypher, char openingCharacter, char closingCharacter)
        {
            cypher = GetCurrentLineOfCypher(cypher);
            if (string.IsNullOrWhiteSpace(cypher))
                return false;

            return cypher.LastIndexOf(closingCharacter) <= cypher.LastIndexOf(openingCharacter) && cypher.LastIndexOf(openingCharacter) != -1;
        } 
        #endregion Is In

        private static bool WasLastSignificantCharASomethingLabel(string cypher, char openingCharacter, Func<string, bool> isInSomethingMethod,  char significantChar = ':')
        {
            if (string.IsNullOrWhiteSpace(cypher) || !isInSomethingMethod(cypher))
                return false;

            var lastIndexOfOpenSquareBracket = cypher.LastIndexOf(openingCharacter);
            var lastIndexOfColon = cypher.LastIndexOf(significantChar);

            if (lastIndexOfColon == -1 || lastIndexOfColon < lastIndexOfOpenSquareBracket)
                return false;

            return true;
        }

        public static bool WasLastSignificantCharARelationshipLabel(string cypher)
        {
           return WasLastSignificantCharASomethingLabel(cypher, '[', IsInARelationship);
        }

        public static bool WasLastSignificantCharANodeLabel(string cypher)
        {
            return WasLastSignificantCharASomethingLabel(cypher, '(', IsInNodeIdentifier);
        }

        #region Helpers
        private static string GetCurrentLineOfCypher(string cypher)
        {
            if (string.IsNullOrWhiteSpace(cypher))
                return cypher;

            var split = cypher.Split(new[] { "\r\n" }, StringSplitOptions.None);
            return split.Length > 1 ? split.Last() : cypher;
        }
        #endregion Helpers
    }
}