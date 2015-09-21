namespace Anabranch.Neo4JConsolePackage
{
    using System.Text;

    public class Neo4jSimpleData : INeo4jData
    {
        public dynamic Data { get; set; }

        private string _toString;

        public override string ToString()
        {
            if(_toString != null)
                return _toString;
            
            var output = new StringBuilder("");
            output.Append(Data.ToString());
            return _toString = output.ToString();
        }

        public int Length { get { return ToString().Length; } }
    }
}