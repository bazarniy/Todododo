using System.Reactive;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using Todododo.Data;

namespace Todododo.ViewModels
{
    public class ToDoViewModel : ReactiveObject
    {
        private readonly ToDo _instance;
        private string _summary;
        private bool _completed;
        private int _depth;

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

        public ToDoViewModel(Node<ToDo, long> node)
        {
            _instance = node.Item;
            _depth = node.Depth;
        }
    }
}
