using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Newtonsoft.Json;

namespace Todododo.Data
{
    public class TodoSortingService
    {
        private const string StorageName = "todododo:todos_sorting";

        private readonly ILocalStorageService _localStorage;
        private readonly Subject<Unit> _changed = new Subject<Unit>();
        private readonly ConcurrentDictionary<long, int> _data = new ConcurrentDictionary<long, int>();

        public IObservable<Unit> SortingChanged => _changed;

        public TodoSortingService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task AddOrUpdate(long id, int sorting)
        {
            _data.AddOrUpdate(id, sorting, (k, v) => sorting);
            
            await SaveLocalStorage();

            _changed.OnNext(Unit.Default);
        }

        public async Task AddOrUpdate(IReadOnlyList<long> ids)
        {
            ids.Select((id, index) => _data.AddOrUpdate(id, index, (k, v) => index)).ToArray();

            await SaveLocalStorage();

            _changed.OnNext(Unit.Default);
        }

        public async Task Remove(long id)
        {
            _data.TryRemove(id, out _);

            await SaveLocalStorage();
        }

        public async Task Remove(IEnumerable<long> ids)
        {
            ids.Select(id => _data.TryRemove(id, out _)).ToArray();

            await SaveLocalStorage();
        }

        public int GetSort(long id) => _data.TryGetValue(id, out var value) ? value : 0;

        public async Task Initialize()
        {
            try
            {
                var data = await _localStorage.GetItemAsStringAsync(StorageName);

                foreach (var (key, value) in JsonConvert.DeserializeObject<Dictionary<long, int>>(data))
                {
                    _data.TryAdd(key, value);
                }

                _changed.OnNext(Unit.Default);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error initializing localstorage {StorageName}");
                Console.WriteLine(e);
            }
        }

        private async Task SaveLocalStorage()
        {
            try
            {
                await _localStorage.SetItemAsync(StorageName, JsonConvert.SerializeObject(_data));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error saving localstorage {StorageName}");
                Console.WriteLine(e);
            }
        }
    }
}