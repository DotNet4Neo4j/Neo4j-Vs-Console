namespace Anabranch.Neo4JConsolePackage
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Neo4jErrorResponse
    {
        public string Message { get; set; }
        public string Exception { get; set; }
        public IEnumerable<string> StackTrace { get; set; }
        public string FullName { get; set; }

        public Neo4jErrorResponse() { StackTrace = new List<string>(); }

        public override string ToString()
        {
            var output = new StringBuilder();

            output.AppendLine(Message.Replace("\n", Environment.NewLine));

            return output.ToString();
        }
    }
}