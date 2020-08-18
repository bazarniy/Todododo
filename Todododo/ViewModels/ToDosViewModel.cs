﻿using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;

namespace Todododo.ViewModels
{
    public class ToDosViewModel : ReactiveObject
    {
        private int _currentCount;

        private readonly ObservableAsPropertyHelper<int> _count;

        public ToDosViewModel()
        {
            Increment = ReactiveCommand.CreateFromTask(IncrementCount);

            _count = Increment.ToProperty(this, x => x.CurrentCount, scheduler: RxApp.MainThreadScheduler);
        }

        public int CurrentCount => _count.Value;
        
        
        public ReactiveCommand<Unit, int> Increment { get; }

        private Task<int> IncrementCount()
        {
            _currentCount++;
            return Task.FromResult(_currentCount);
        }
    }
}