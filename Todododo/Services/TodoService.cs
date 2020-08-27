﻿using System.Collections.Generic;
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

            _data.AddOrUpdate(dto);

            await _localStorage.SetItemAsync(StorageName, _data.Items.ToArray());
        }

        public async Task Remove(ToDo dto)
        {
            if (dto.Id == default) return;

            _data.Remove(ItemsToRemove(dto.Id));

            await _localStorage.SetItemAsync(StorageName, _data.Items.ToArray());
        }

        private async Task Load()
        {
            var items = await _localStorage.GetItemAsync<List<ToDo>>(StorageName);
            _data.AddOrUpdate(items);
        }

        private IEnumerable<long> ItemsToRemove(long id) => _data.Items
            .Where(x => x.ParentId == id)
            .SelectMany(x => ItemsToRemove(x.Id))
            .Concat(new[] {id});
    }
}