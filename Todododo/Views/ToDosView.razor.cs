using ReactiveUI.Blazor;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Todododo.ViewModels;

namespace Todododo.Views
{
    public class ToDosViewBase: ReactiveComponentBase<ToDosViewModel>
    {
        public ToDosViewBase()
        {
            ViewModel = new ToDosViewModel();
        }

        protected async Task IncrementCount()
        {
            await ViewModel.Increment.Execute().ToTask();
        }
    }
}
