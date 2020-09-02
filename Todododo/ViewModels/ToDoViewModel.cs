using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DynamicData;
using DynamicData.Binding;
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

        public long Id { get; private set; }
        public long ParentId { get; private set; }

        public string Summary
        {
            get => _summary;
            set => this.RaiseAndSetIfChanged(ref _summary, value);
        }

        public bool Completed
        {
            get => _completed;
            set => this.RaiseAndSetIfChanged(ref _completed, value);
        }

        public bool IsEdit => _isEdit.Value;
        
        public bool CanSave => _canSave.Value;
        
        public ReactiveCommand<Unit, Unit> Edit { get; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> Remove { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public ToDoViewModel(Node<ToDo, long> node, TodoService service, IMapper mapper)
        {
            _depth = node.Depth;

            mapper.Map(node.Item, this);

            var unchangedTodo = mapper.Map<ToDo>(this);

            Edit = ReactiveCommand.Create(() => { unchangedTodo = mapper.Map<ToDo>(this); });
            Cancel = ReactiveCommand.Create(() => { mapper.Map(unchangedTodo, this); });

            Save = ReactiveCommand.CreateFromTask(
                async () => await service.AddOrUpdate(mapper.Map<ToDo>(this)),
                this.WhenAnyValue(x => x.Summary).Select(x => !string.IsNullOrWhiteSpace(x))
            );

            Remove = ReactiveCommand.CreateFromTask(async x => await service.Remove(node.Key));

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
