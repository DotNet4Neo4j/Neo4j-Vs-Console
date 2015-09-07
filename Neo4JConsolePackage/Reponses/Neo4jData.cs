namespace Anabranch.Neo4JConsolePackage
{
    using System.Collections.Generic;
    using System.Text;

    public class Neo4jData
    {
        private string _combined;

        public Neo4jData()
        {
            Data = new Dictionary<string, string>();
        }

        private string Combined
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_combined))
                    _combined = GenerateCombined(Data);
                return _combined;
            }
        }

        public IDictionary<string, string> Data { get; set; }

        internal int Length
        {
            get { return Combined.Length; }
        }

        private static string GenerateCombined(ICollection<KeyValuePair<string, string>> data)
        {
            if (data.Count == 0)
                return "{}";

            var output = new StringBuilder("{");

            foreach (var kvp in data)
                output.Append(string.Format("{0}:\"{1}\",", kvp.Key, kvp.Value));

            output.Remove(output.Length - 1, 1).Append("}");

            return output.ToString();
        }

        public override string ToString()
        {
            return Combined;
        }
    }
}