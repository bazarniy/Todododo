using Todododo.ViewModels;

namespace Todododo.Helpers
{
    public class DragAndDropContainer
    {
        public ToDoViewModel MovedData { get; set; }

        public ToDoViewModel Take()
        {
            var data = MovedData;
            MovedData = null;
            return data;
        }
    }
}