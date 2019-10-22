namespace LmsApi
{
    public class TableResult<T>
    {
        public T[] Data { get; set; }
        public string[] Header { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return $"{Title} ({Data.Length} rows)";
        }
    }
}
