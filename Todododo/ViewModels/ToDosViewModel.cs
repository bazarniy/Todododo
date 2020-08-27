using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using Todododo.Data;

namespace Todododo.ViewModels
{
    public class ToDosViewModel : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _cleanup = new CompositeDisposable();
        private readonly ReadOnlyObservableCollection<ToDoViewModel> _data;

        public ReadOnlyObservableCollection<ToDoViewModel> Data => _data;

        public ReactiveCommand<Unit, Unit> Add { get; }
        public ReactiveCommand<ToDoViewModel, Unit> Remove { get; }

        public ToDosViewModel(TodoService service)
        {
            static bool DefaultPredicate(Node<ToDo, long> node) => node.IsRoot;

            service.Todos.Connect()
                .TransformToTree(x => x.ParentId, Observable.Return((Func<Node<ToDo, long>, bool>)DefaultPredicate))
                .Transform(node => new ToDoViewModel(node))
                .Bind(out _data)
                .DisposeMany()
                .Subscribe()
                .DisposeWith(_cleanup);

            Add = ReactiveCommand
                .CreateFromTask(() => service.AddOrUpdate(new ToDo { Summary = "Added todo" }))
                .DisposeWith(_cleanup);

            Remove = ReactiveCommand
                .CreateFromTask<ToDoViewModel, Unit>(async x =>
                {
                    await service.Remove(x.Current());
                    return Unit.Default;
                })
                .DisposeWith(_cleanup);
        }

        public void Dispose() => _cleanup.Dispose();
    }
}
