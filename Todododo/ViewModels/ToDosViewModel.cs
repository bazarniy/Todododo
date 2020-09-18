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
        private ReadOnlyObservableCollection<ToDoViewModel> _data;

        public ReadOnlyObservableCollection<ToDoViewModel> Data => _data;


        public ReactiveCommand<Unit, Unit> Add { get; }

        public ToDosViewModel(Func<Node<ToDo, long>, ToDoViewModel> createViewModel, TodoServiceFacade service)
        {
            service.Todos //TODO: не обновления при изменении сортировки. То ли прилетает изменение коллекции и одновременно сортировка, то ли я хз
                .TransformToTree(x => x.ParentId)
                .Filter(x => !x.Item.Completed)
                .Sort(
                    SortExpressionComparer<Node<ToDo, long>>.Ascending(p =>
                    {
                        var s = service.GetSort(p.Key);
                        Console.WriteLine($"Sort for {p.Key} is {s}");
                        return s;
                    }),
                    service.SortingChanged
                )
                .Transform(createViewModel)
                .Bind(out _data)
                .DisposeMany()
                .Subscribe()
                .DisposeWith(_cleanup);

            service.SortingChanged.Subscribe(_ => Console.WriteLine("SORTING CHANGED"));

            Add = ReactiveCommand.CreateFromTask(async () =>
            {
                await service.AddOrUpdate(new ToDo(), Data.Count);
            });
        }

        public void Dispose() => _cleanup.Dispose();
    }
}
