namespace Todododo.Data
{
    public class ToDo
    {
        public long Id { get; set; }
        public long ParentId { get; set; }

        public bool Completed { get; set; }

        public string Summary { get; set; }
    }
}
