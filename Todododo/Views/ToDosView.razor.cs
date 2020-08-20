using Microsoft.AspNetCore.Components;
using ReactiveUI.Blazor;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Todododo.ViewModels;

namespace Todododo.Views
{
    public class ToDosViewBase: ReactiveInjectableComponentBase<ToDosViewModel>
    {
        public ToDosViewBase()
        {
            
        }

        protected async Task Add()
        {
            await ViewModel.Add.Execute().ToTask();
        }
    }
}
