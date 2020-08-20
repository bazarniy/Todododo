using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Todododo.Data;

namespace Todododo.ViewModels
{
    public class ToDoViewModel : ReactiveObject
    {
        private readonly ToDo _instance;
        private string _summary;
        private bool _completed;

        public string Summary
        {
            get => _instance.Summary;
            set => _instance.Summary = this.RaiseAndSetIfChanged(ref _summary, value);
        }
        public bool Completed
        {
            get => _instance.Completed;
            set => _instance.Completed = this.RaiseAndSetIfChanged(ref _completed, value);
        }

        public ToDo Current() => _instance;

        public ToDoViewModel(ToDo instance = null)
        {
            _instance = instance ?? new ToDo();
        }
    }
}
