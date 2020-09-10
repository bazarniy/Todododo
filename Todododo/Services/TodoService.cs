using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using DynamicData;
using IdGen;

namespace Todododo.Data
{
    public class TodoService
    {
        private const string StorageName = "todododo:todos";

        private readonly IIdGenerator<long> _idGen;
        private readonly ILocalStorageService _localStorage;
        private readonly SourceCache<ToDo, long> _data = new SourceCache<ToDo, long>(x => x.Id);

        public TodoService(IIdGenerator<long> idGen, ILocalStorageService localStorage)
        {
            _idGen = idGen;
            _localStorage = localStorage;

            Task.Run(Load);
        }

        public IObservableCache<ToDo, long> Todos => _data.AsObservableCache();

        public async Task AddOrUpdate(ToDo dto)
        {
            if (dto.Id == default) dto.Id = _idGen.CreateId();

            Console.WriteLine($"AddOrUpdate id {dto.Id} parentId {dto.ParentId} summary {dto.Summary}");
            _data.AddOrUpdate(dto);

            await _localStorage.SetItemAsync(StorageName, _data.Items.ToArray());
        }

        public async Task Complete(long id)
        {
            //todo: вот это должно удалять элементы из основного хранилища и выкидывать в отдельное
            var ids = FlattenThisAndChildren(id).ToArray();
            var items = _data.Items
                .Where(x => !x.Completed && ids.Contains(x.Id))
                .Select(x =>
                {
                    x.Completed = true;
                    return x;
                });

            _data.AddOrUpdate(items);

            await _localStorage.SetItemAsync(StorageName, _data.Items.ToArray());
        }

        public async Task Remove(long id)
        {
            if (id == default) return;

            _data.Remove(FlattenThisAndChildren(id));

            await _localStorage.SetItemAsync(StorageName, _data.Items.ToArray());
        }

        private async Task Load()
        {
            var items = await _localStorage.GetItemAsync<IEnumerable<ToDo>>(StorageName);
            _data.AddOrUpdate(items);
        }

        private IEnumerable<long> FlattenThisAndChildren(long id) => _data.Items
            .Where(x => x.ParentId == id)
            .SelectMany(x => FlattenThisAndChildren(x.Id))
            .Concat(new[] {id});
    }
}