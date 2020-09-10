using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Components;
using ReactiveUI.Blazor;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;
using Blazorise;
using Blazorise.Icons.Material;
using DynamicData.Binding;
using Microsoft.AspNetCore.Components.Web;
using ReactiveUI;
using Todododo.Helpers;
using Todododo.ViewModels;

namespace Todododo.Views
{
    public class TodoViewBase: ReactiveComponentBase<ToDoViewModel>
    {
        [Inject]
        protected DragAndDropContainer DragAndDropContainer { get; set; }

        protected string DropClass { get; set; }
        protected string DropNextClass { get; set; }
        protected string DropBeforeClass { get; set; }

        protected bool IsCompleted
        {
            get => ViewModel?.Completed ?? false;
            set => ViewModel?.Complete.Execute();
        }

        protected IFluentSpacingOnBreakpointWithSideAndSize Indent => ViewModel.Depth switch
        {
            0 => Margin.Is0.FromLeft,
            1 => Margin.Is2.FromLeft,
            2 => Margin.Is4.FromLeft,
            _ => Margin.Is5.FromLeft
        };

        protected void OnDragStart() => DragAndDropContainer.MovedData = ViewModel;

        protected async Task OnDropBefore()
        {
            DropBeforeClass = "";
            await Drop(ToDoViewModel.DropState.Before);
        }

        protected async Task OnDrop()
        {
            DropClass = "";
            await Drop(ToDoViewModel.DropState.Here);
        }

        protected async Task OnDropNext()
        {
            DropNextClass = "";
            await Drop(ToDoViewModel.DropState.After);
        }

        protected void OnDragBeforeEnter() => DropBeforeClass = DragAndDropContainer.MovedData != ViewModel ? "can-drop": DropBeforeClass;

        protected void OnDragEnter() => DropClass = "can-drop";

        protected void OnDragNextEnter() => DropNextClass = DragAndDropContainer.MovedData != ViewModel ? "can-drop": DropNextClass;

        protected void OnDragBeforeLeave() => DropBeforeClass = "";

        protected void OnDragLeave() => DropClass = "";

        protected void OnDragNextLeave() => DropNextClass = "";

        private async Task Drop(ToDoViewModel.DropState where)
        {
            var data = DragAndDropContainer.Take();
            if (data == ViewModel) return;
            await ViewModel.Drop.Execute((where, data));
        }
    }
}
