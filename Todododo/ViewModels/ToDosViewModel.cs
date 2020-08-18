using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Todododo.Data;

namespace Todododo.ViewModels
{
    public class ToDosViewModel : ReactiveObject
    {
        public ReadOnlyObservableCollection<ToDoViewModel> Data { get; }

        public ReactiveCommand<Unit, Unit> Add { get; }

        public ToDosViewModel()
        {
            var collection = new ObservableCollection<ToDoViewModel>(new[]
            {
                new ToDoViewModel(new ToDo(){Summary="First todo"}),
                new ToDoViewModel(new ToDo(){Summary="Second todo", Completed=true})
            });
            Data = new ReadOnlyObservableCollection<ToDoViewModel>(collection);

            Add = ReactiveCommand.Create(() => collection.Add(new ToDoViewModel(new ToDo() { Summary = "Added todo" })));
        }
    }
}
