using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using Todododo.Data;
using Todododo.Helpers;

namespace Todododo.ViewModels
{
    public class ToDoViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> _canSave;
        private readonly ObservableAsPropertyHelper<bool> _isEdit;
        private string _summary;
        private bool _completed;
        private int _depth;

        private ToDo _instance;
        private ToDo _unchangedTodo;

        public string Summary
        {
            get => _summary;
            set
            {
                _instance.Summary = value;
                this.RaiseAndSetIfChanged(ref _summary, value);
            }
        }

        public bool Completed
        {
            get => _completed;
            set
            {
                _instance.Completed = value;
                this.RaiseAndSetIfChanged(ref _completed, value);
            }
        }

        public bool IsEdit => _isEdit.Value;
        
        public bool CanSave => _canSave.Value;
        
        public ReactiveCommand<Unit, Unit> Edit { get; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> Remove { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public ToDoViewModel(Node<ToDo, long> node, TodoService service)
        {
            _instance = node.Item;
            _unchangedTodo = _instance.DeepClone();
            _depth = node.Depth;

            Summary = _instance.Summary;
            Completed = _instance.Completed;

            Edit = ReactiveCommand.Create(
                () => { _unchangedTodo = _instance.DeepClone(); }
            );

            Save = ReactiveCommand.CreateFromTask(
                async () => await service.AddOrUpdate(_instance),
                this.WhenAnyValue(x => x.Summary).Select(x => !string.IsNullOrWhiteSpace(x))
            );

            Cancel = ReactiveCommand.Create(() =>
            {
                _instance = _unchangedTodo;
                Summary = _instance.Summary;
                Completed = _instance.Completed;
            });

            Remove = ReactiveCommand.CreateFromTask(
                async x => await service.Remove(node.Key)
            );

            Save
                .CanExecute
                .ToProperty(this, x => x.CanSave, out _canSave);

            var isEdit = Edit
                .Select(x => true)
                .Merge(Save.Select(x => false))
                .Merge(Cancel.Select(x => false));

            isEdit
                .Where(x => !x && string.IsNullOrWhiteSpace(Summary))
                .Select(_ => Unit.Default)
                .InvokeCommand(Remove);

            isEdit.ToProperty(this, x => x.IsEdit, out _isEdit, string.IsNullOrWhiteSpace(Summary));

            this.WhenAnyPropertyChanged(nameof(Completed)) //this.WhenAnyValue(vm => vm.Completed) there are "RuntimeError: memory access out of bounds" in the browser
                .Select(_ => Unit.Default)
                .InvokeCommand(Save);

            
        }
    }
}
