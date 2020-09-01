using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
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

        public ToDosViewModel(TodoService service)
        {
            //static bool DefaultPredicate(Node<ToDo, long> node) => node.IsRoot;

            service.Todos.Connect()
                .TransformToTree(x => x.ParentId/*, Observable.Return((Func<Node<ToDo, long>, bool>) DefaultPredicate)*/)
                .Transform(node => new ToDoViewModel(node, service))
                .Bind(out _data)
                .Subscribe()
                .DisposeWith(_cleanup);

            Add = ReactiveCommand.CreateFromTask(() => service.AddOrUpdate(new ToDo()));
        }

        public void Dispose() => _cleanup.Dispose();
    }
}
