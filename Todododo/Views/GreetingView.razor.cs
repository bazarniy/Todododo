using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Todododo.ViewModels;

namespace Todododo.Views
{
    public partial class GreetingView
    {
        public GreetingView()
        {
            ViewModel = new GreetingViewModel();
        }

        public async Task Clear()
        {
            await ViewModel.Clear.Execute().ToTask();
        }
    }
}
