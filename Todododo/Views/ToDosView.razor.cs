using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Components;
using ReactiveUI.Blazor;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DynamicData.Binding;
using ReactiveUI;
using Todododo.ViewModels;

namespace Todododo.Views
{
    public class ToDosViewBase: ReactiveInjectableComponentBase<ToDosViewModel>
    {
        public ToDosViewBase()
        {
            this.WhenActivated(disposable =>
            {
                ViewModel.DisposeWith(disposable);

                ViewModel!.Data
                    .ObserveCollectionChanges()
                    .Subscribe(async _ =>
                    {
                        Console.WriteLine("Redraw");
                        await InvokeAsync(StateHasChanged);
                    })
                    .DisposeWith(disposable);
            });
        }
    }
}
