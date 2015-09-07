namespace Anabranch.Neo4JConsolePackage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Neo4jResponse
    {
        private string _sep;

        public Neo4jResponse()
        {
            Columns = new List<string>();
            Data = new List<Neo4jData>();
        }

        public IList<string> Columns { get; set; }
        public IList<Neo4jData> Data { get; set; }

        public override string ToString()
        {

            var output = new StringBuilder();
            if (!Data.Any())
                return string.Format("+-------------------+{0}| No data returned. |{0}+-------------------+", Environment.NewLine);

            if (Data.Count % Columns.Count != 0)
                return string.Format("ERROR: The data appears to be corrupt, we have {0} columns, but {1} bits of data.", Columns.Count, Data.Count);


            List<int> lengths = Data.Select(neo4JData => neo4JData.Length).ToList();

            output.AppendLine(WriteSeparator(lengths));
            output.AppendLine(WriteColumns(lengths));
            output.AppendLine(WriteSeparator(lengths));

            output.Append(GetRows(lengths.Max()));

            output.Append(WriteSeparator(lengths));

            return output.ToString();
        }

        private string GetRows(int maxLength)
        {
            var output = new StringBuilder();
            for (int i = 0; i < Data.Count; i += Columns.Count)
            {
                string row = GetRow(i, Columns.Count, maxLength);
                output.AppendLine(row);
            }
            return output.ToString();
        }

        private string GetRow(int startPoint, int count, int maxLength)
        {
            var row = new StringBuilder("| ");

            for (int i = startPoint; i < startPoint + count; i++)
                row.Append(Data[i].ToString().PadRight(maxLength)).Append(" | ");

            return row.ToString();
        }

        private string WriteColumns(IEnumerable<int> lengths)
        {
            int maxLength = lengths.Max();
            var output = new StringBuilder();

            foreach (string columnName in Columns)
                output.Append("| ").Append(columnName.PadRight(maxLength+ 1));

            return output.Append("|").ToString();
        }

        private string WriteSeparator(ICollection<int> lengths)
        {
            if (!string.IsNullOrEmpty(_sep))
                return _sep;

            var output = new StringBuilder();

            for(int i = 0; i < Columns.Count; i++)
                output.Append("+").Append(new string('-', lengths.Max() + 2));

            return _sep = output.Append("+").ToString();
        }
    }
}