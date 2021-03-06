﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Todododo.Data;
using Todododo.Helpers;

namespace Todododo.ViewModels
{
    public class ToDoViewModel : ReactiveObject, IDisposable
    {
        public enum DropState
        {
            Before,
            Here,
            After
        }

        private readonly IDisposable _cleanup;
        private readonly ObservableAsPropertyHelper<bool> _canSave;
        private readonly ObservableAsPropertyHelper<bool> _isEdit;
        private readonly ObservableAsPropertyHelper<bool> _canExpand;

        private string _summary;
        private ReadOnlyObservableCollection<ToDoViewModel> _children;
        private bool _isExpanded;

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public long Id { get; private set; } 
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public long ParentId { get; private set; }

        public bool Completed { get; private set; }

        public int Depth { get; }

        public string Summary
        {
            get => _summary;
            set => this.RaiseAndSetIfChanged(ref _summary, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public bool CanExpand => _canExpand.Value;

        public bool IsEdit => _isEdit.Value;
        
        public bool CanSave => _canSave.Value;

        public ReadOnlyObservableCollection<ToDoViewModel> Children => _children;

        public ReactiveCommand<Unit, Unit> Expand { get; }
        public ReactiveCommand<Unit, Unit> Edit { get; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> Complete { get; }
        public ReactiveCommand<Unit, Unit> Remove { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public ReactiveCommand<(DropState, ToDoViewModel), Unit> Drop { get; }

        public ToDoViewModel(Node<ToDo, long> node, TodoServiceFacade service, IMapper mapper)
        {
            Depth = node.Depth;

            var childrenLoader = new Lazy<IDisposable>(() =>
                node.Children.Connect()
                    .Sort(
                        SortExpressionComparer<Node<ToDo, long>>.Ascending(p => service.GetSort(p.Key)),
                        service.SortingChanged
                    )
                    .Transform(e => new ToDoViewModel(e, service, mapper))
                    .Bind(out _children)
                    .DisposeMany()
                    .Subscribe()
            );

            var expander = this.WhenValueChanged(x=>x.IsExpanded)
                .Where(isExpanded => isExpanded)
                .Take(1)
                .Subscribe(_ => { var x = childrenLoader.Value; }); //force lazy loading

            mapper.Map(node.Item, this);

            Expand = ReactiveCommand.Create(
                () => { IsExpanded = !IsExpanded; },
                node.Children.CountChanged.Select(x => x > 0)
            );

            Expand
                .CanExecute
                .ToProperty(this, x => x.CanExpand, out _canExpand);

            var unchangedTodo = mapper.Map<ToDo>(this);
            Edit = ReactiveCommand.Create(() => { unchangedTodo = mapper.Map<ToDo>(this); });
            Cancel = ReactiveCommand.Create(() => { mapper.Map(unchangedTodo, this); });

            Save = ReactiveCommand.CreateFromTask(
                async () => { await service.AddOrUpdate(mapper.Map<ToDo>(this)); },
                this.WhenAnyValue(x => x.Summary).Select(x => !string.IsNullOrWhiteSpace(x))
            );

            Complete = ReactiveCommand.CreateFromTask(async () => await service.Complete(node.Key));

            Remove = ReactiveCommand.CreateFromTask(async x => await service.Remove(node.Key));

            Drop = ReactiveCommand.CreateFromTask<(DropState, ToDoViewModel), Unit>(async (x, _) =>
            {
                var (dropstate, vm) = x;
                if (vm == null || vm == this) return Unit.Default;

                var item = mapper.Map<ToDo>(vm);
                
                item.ParentId = dropstate == DropState.Here
                    ? node.Key
                    : node.Parent.HasValue ? node.Parent.Value.Key : 0;

                switch (dropstate)
                {
                    case DropState.Here:
                        await service.AddOrUpdate(item, node.Children.Count);
                        break;
                    case DropState.After:
                        await service.AddOrUpdateAfter(item, node.Key);
                        break;
                    case DropState.Before:
                        await service.AddOrUpdateBefore(item, node.Key);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return Unit.Default;
            });

            Save
                .CanExecute
                .ToProperty(this, x => x.CanSave, out _canSave);

            var isEdit = Edit
                .Select(x => true)
                .Merge(Save.Select(x => false))
                .Merge(Cancel.Select(x => false));

            isEdit
                .Where(x => !x && string.IsNullOrWhiteSpace(Summary))
                .Select(_ => Unit.Default)
                .InvokeCommand(Remove);

            isEdit.ToProperty(this, x => x.IsEdit, out _isEdit, string.IsNullOrWhiteSpace(Summary));

            _cleanup = Disposable.Create(() =>
            {
                expander.Dispose();
                if (childrenLoader.IsValueCreated)
                    childrenLoader.Value.Dispose();
            });
        }

        public void Dispose() => _cleanup.Dispose();
    }
}
