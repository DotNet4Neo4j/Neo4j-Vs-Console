namespace Anabranch.Neo4JConsolePackage.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class CollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}