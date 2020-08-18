using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Todododo.Data;

namespace Todododo.ViewModels
{
    public class ToDoViewModel : ReactiveObject
    {
        private readonly ToDo _instance;

        public string Summary => _instance.Summary;
        public bool Completed { get; set; } 

        public ToDoViewModel(ToDo instance = null)
        {
            _instance = instance ?? new ToDo();
            Completed = _instance.Completed;
        }
    }
}
