using System;
using System.Collections.Generic;

namespace Todododo.Data
{
    public class ToDo
    {
        private sealed class ToDoEqualityComparer : IEqualityComparer<ToDo>
        {
            public bool Equals(ToDo x, ToDo y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id && x.ParentId == y.ParentId && x.Completed == y.Completed && x.Summary == y.Summary;
            }

            public int GetHashCode(ToDo obj)
            {
                return HashCode.Combine(obj.Id, obj.ParentId, obj.Completed, obj.Summary);
            }
        }

        public static IEqualityComparer<ToDo> ToDoComparer { get; } = new ToDoEqualityComparer();

        public long Id { get; set; }
        public long ParentId { get; set; }

        public bool Completed { get; set; }

        public string Summary { get; set; }
    }
}
