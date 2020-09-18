using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DynamicData;


namespace Todododo.Data
{
    public class TodoServiceFacade
    {
        private readonly TodoService _todoService;
        private readonly TodoSortingService _sortingService;

        public IObservable<IChangeSet<ToDo, long>> Todos { get; }
        public IObservable<Unit> SortingChanged => _sortingService.SortingChanged;

        public TodoServiceFacade(TodoService todoService, TodoSortingService sortingService)
        {
            _todoService = todoService;
            _sortingService = sortingService;

            Todos = Observable
                .When(
                    _sortingService.Initialize().ToObservable()
                        .And(_todoService.Initialize().ToObservable())
                        .Then((l, r) => true)
                )
                .StartWith(false)
                .Select(x => !x
                    ? Observable.Never<IObservableCache<ToDo, long>>()
                    : Observable.Return(_todoService.Todos)
                )
                .Switch()
                .Switch(); //todo: проверить будет ли эта штука жить вечно

        }

        public async Task AddOrUpdate(ToDo dto, int sorting = 0)
        {
            var id = await _todoService.AddOrUpdate(dto);
            
            await _sortingService.AddOrUpdate(id, sorting != 0 ? sorting : _sortingService.GetSort(id));
        }

        public Task AddOrUpdateAfter(ToDo dto, long anchorId) => AddOrUpdateAnchor(dto, anchorId, true);
        
        public Task AddOrUpdateBefore(ToDo dto, long anchorId) => AddOrUpdateAnchor(dto, anchorId, false);
        
        private async Task AddOrUpdateAnchor(ToDo dto, long anchorId, bool after = false)
        {
            var id = await _todoService.AddOrUpdate(dto);

            var children = _todoService.Todos.Items
                .Where(x => x.ParentId == dto.ParentId)
                .Select(x => x.Id)
                .OrderBy(x => _sortingService.GetSort(x))
                .ToList();

            children.Remove(id);
            if (!after) children.Insert(children.IndexOf(anchorId), id);
            else children.Insert(children.IndexOf(anchorId) + 1, id);

            await _sortingService.AddOrUpdate(children);
        }

        public async Task Complete(long id)
        {
            await _todoService.Complete(id);
            await _sortingService.Remove(id);
        }

        public async Task Remove(long id)
        {
            await _todoService.Remove(id);
            await _sortingService.Remove(id);
        }

        public int GetSort(long id) => _sortingService.GetSort(id);
    }
}