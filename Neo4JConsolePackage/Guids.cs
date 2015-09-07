// Guids.cs
// MUST match guids.h
using System;

namespace Anabranch.Neo4JConsolePackage
{
    static class GuidList
    {
        public const string guidNeo4JConsolePackagePkgString = "f79a4146-8c54-4c7b-b1b6-b8569e18211b";
        public const string guidNeo4JConsolePackageCmdSetString = "0c655f97-bfde-4f11-a56b-cd1272d563ba";
        public const string guidToolWindowPersistanceString = "2704ff00-4ca9-4802-8131-7f42a08d322d";

        public static readonly Guid guidNeo4JConsolePackageCmdSet = new Guid(guidNeo4JConsolePackageCmdSetString);
    };
}