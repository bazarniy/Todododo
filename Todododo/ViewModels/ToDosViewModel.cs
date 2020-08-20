using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using DynamicData;
using DynamicData.Binding;
using LiteDB;
using ReactiveUI;
using Todododo.Data;

namespace Todododo.ViewModels
{
    public class ToDosViewModel : ReactiveObject
    {
        public ReadOnlyObservableCollection<ToDoViewModel> Data { get; }

        public ReactiveCommand<Unit, Unit> Add { get; }

        public ToDosViewModel(ILocalStorageService storage)
        {
            var collection = new ObservableCollection<ToDoViewModel>();
            Data = new ReadOnlyObservableCollection<ToDoViewModel>(collection);

            Observable
                .StartAsync(async _ => await storage.DbStream())
                .Subscribe(stream =>
                {
                    //не флашит в стрим, без диспоза. Наверное можно просто в жсон переделать, чтобы не трахаться
                    using (var db = new LiteDatabase(stream))
                    {
                        collection.AddRange(db.GetCollection<ToDo>("todo").FindAll().Select(x => new ToDoViewModel(x)));
                    }

                    collection.ActOnEveryObject(
                    x =>
                    {

                        using (var db = new LiteDatabase(stream))
                        {
                            db.GetCollection<ToDo>("todo").Insert(x.Current());
                        }

                        stream.Flush();
                    },
                    x =>
                    {

                    });
                });
            



            /*collection.AddRange(new[]
            {
                new ToDoViewModel(new ToDo(){Summary="First todo"}),
                new ToDoViewModel(new ToDo(){Summary="Second todo", Completed=true})
            });*/

            Add = ReactiveCommand.Create(() => collection.Add(new ToDoViewModel(new ToDo() { Summary = "Added todo" })));
        }
    }
}
