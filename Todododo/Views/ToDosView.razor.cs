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
        private readonly Subject<Unit> _add = new Subject<Unit>();
        private readonly Subject<ToDoViewModel> _remove = new Subject<ToDoViewModel>();
        private readonly Subject<ToDoViewModel> _edit = new Subject<ToDoViewModel>();

        public ToDosViewBase()
        {
            this.WhenActivated(disposable =>
            {
                ViewModel.DisposeWith(disposable);

                ViewModel!.Data
                    .ObserveCollectionChanges()
                    .Subscribe(async _ => await InvokeAsync(StateHasChanged))
                    .DisposeWith(disposable);

                _add
                    .InvokeCommand(ViewModel!.Add)
                    .DisposeWith(disposable);

                _remove
                    .InvokeCommand(ViewModel.Remove)
                    .DisposeWith(disposable);
            });
        }

        protected void Add() => _add.OnNext(Unit.Default);
        protected void Edit(ToDoViewModel x) => _edit.OnNext(x);
        protected void Remove(ToDoViewModel x) => _remove.OnNext(x);
    }
}
