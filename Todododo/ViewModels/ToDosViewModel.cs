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

        public ToDosViewModel(Func<Node<ToDo, long>, ToDoViewModel> createViewModel, TodoService service)
        {
            static bool DefaultPredicate(Node<ToDo, long> node) => node.IsRoot;

            service.Todos.Connect()
                .TransformToTree(x => x.ParentId/*, Observable.Return((Func<Node<ToDo, long>, bool>) DefaultPredicate)*/)
                .Transform(createViewModel)
                .Bind(out _data)
                .DisposeMany()
                .Subscribe()
                .DisposeWith(_cleanup);

            Add = ReactiveCommand.CreateFromTask(() => service.AddOrUpdate(new ToDo()));
        }

        public void Dispose() => _cleanup.Dispose();
    }
}
