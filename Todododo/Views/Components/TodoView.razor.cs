using System;
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
using ReactiveUI;
using Todododo.ViewModels;

namespace Todododo.Views
{
    public class TodoViewBase: ReactiveComponentBase<ToDoViewModel>
    {


        protected IFluentSpacingOnBreakpointWithSideAndSize Indent => ViewModel.Depth switch
        {
            0 => Margin.Is0.FromLeft,
            1 => Margin.Is2.FromLeft,
            2 => Margin.Is4.FromLeft,
            _ => Margin.Is5.FromLeft

        };

    }
}
