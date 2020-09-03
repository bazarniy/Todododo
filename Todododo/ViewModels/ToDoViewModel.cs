using System;
using System.Collections.ObjectModel;
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
    public class ToDoViewModel : ReactiveObject, IDisposable
    {
        private readonly IDisposable _cleanup;
        private readonly ObservableAsPropertyHelper<bool> _canSave;
        private readonly ObservableAsPropertyHelper<bool> _isEdit;
        private readonly ObservableAsPropertyHelper<bool> _canExpand;

        private string _summary;
        private bool _completed;
        private ReadOnlyObservableCollection<ToDoViewModel> _children;
        private bool _isExpanded;

        public long Id { get; private set; }
        public long ParentId { get; private set; }
        public int Depth { get; private set; }

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

        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public bool CanExpand => _canExpand.Value;

        public bool IsEdit => _isEdit.Value;
        
        public bool CanSave => _canSave.Value;

        public ReadOnlyObservableCollection<ToDoViewModel> Children => _children;

        public ReactiveCommand<Unit, Unit> Expand { get; }
        public ReactiveCommand<Unit, Unit> Edit { get; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> Remove { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public ToDoViewModel(Node<ToDo, long> node, TodoService service, IMapper mapper, ToDoViewModel parent = null)
        {
            Depth = node.Depth;

            var childrenLoader = new Lazy<IDisposable>(() =>
                node.Children.Connect()
                    .Transform(e => new ToDoViewModel(e, service, mapper, this))
                    .Bind(out _children)
                    .DisposeMany()
                    .Subscribe()
            );

            /*var shouldExpand = node.IsRoot || IsExpanded
                ? Observable.Return(true)
                : parent.WhenValueChanged(x => x.IsExpanded);*/

            var expander = this.WhenValueChanged(x=>x.IsExpanded)
                .Where(isExpanded => isExpanded)
                .Take(1)
                .Subscribe(_ =>
                {
                    //force lazy loading
                    var x = childrenLoader.Value;
                });

            Console.WriteLine(Summary + " IsExpanded: " + IsExpanded);
            mapper.Map(node.Item, this);
            Console.WriteLine(Summary + " IsExpanded: " + IsExpanded);


            Expand = ReactiveCommand.Create(
                () => { IsExpanded = !IsExpanded; },
                node.Children.CountChanged.Select(x => x > 0)
            );

            Expand
                .CanExecute
                .ToProperty(this, x => x.CanExpand, out _canExpand);

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

            _cleanup = Disposable.Create(() =>
            {
                expander.Dispose();
                if (childrenLoader.IsValueCreated)
                    childrenLoader.Value.Dispose();
            });
        }

        public void Dispose() => _cleanup.Dispose();
    }
}
