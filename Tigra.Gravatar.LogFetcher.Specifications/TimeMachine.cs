// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: TimeMachine.cs  Created: 2013-07-09@16:45
// Last modified: 2013-07-09@16:46 by Tim

/*
 * This code (TimeMachine) courtesy of Jon Skeet, C# In Depth, Ch. 15
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    public sealed class TimeMachine
        {
        readonly SortedDictionary<int, Action> actions = new SortedDictionary<int, Action>();

        public Task<T> ScheduleSuccess<T>(int time, T value)
            {
            return AddAction<T>(time, tcs => tcs.SetResult(value));
            }

        public Task<T> ScheduleFault<T>(int time, Exception exception)
            {
            return AddAction<T>(time, tcs => tcs.SetException(exception));
            }

        public Task<T> ScheduleFault<T>(int time, IEnumerable<Exception> exceptions)
            {
            return AddAction<T>(time, tcs => tcs.SetException(exceptions));
            }

        public Task<T> ScheduleCancellation<T>(int time)
            {
            return AddAction<T>(time, tcs => tcs.SetCanceled());
            }

        Task<T> AddAction<T>(int time, Action<TaskCompletionSource<T>> action)
            {
            if (time <= 0)
                throw new ArgumentOutOfRangeException("time", "Tasks can only be scheduled with a positive time");
            if (actions.ContainsKey(time))
                throw new ArgumentException("A task completing at this time has already been scheduled.", "time");
            var source = new TaskCompletionSource<T>();
            actions[time] = () => action(source);
            return source.Task;
            }

        public void ExecuteInContext(Action<Advancer> action)
            {
            ExecuteInContext(new ManuallyPumpedSynchronizationContext(), action);
            }

        public void ExecuteInContext(ManuallyPumpedSynchronizationContext context, Action<Advancer> action)
            {
            SynchronizationContext originalContext = SynchronizationContext.Current;
            try
                {
                SynchronizationContext.SetSynchronizationContext(context);
                var advancer = new Advancer(actions, context);
                // This is where the tests assertions etc will go...
                action(advancer);
                }
            finally
                {
                SynchronizationContext.SetSynchronizationContext(originalContext);
                }
            }

        // So tempted to call this class SonicScrewdriver...
        public class Advancer
            {
            readonly SortedDictionary<int, Action> actions;
            readonly ManuallyPumpedSynchronizationContext context;
            int time;

            internal Advancer(SortedDictionary<int, Action> actions, ManuallyPumpedSynchronizationContext context)
                {
                this.actions = actions;
                this.context = context;
                }

            public int Time { get { return time; } }

            /// <summary>
            ///   Advances to the given target time.
            /// </summary>
            /// <param name="targetTime"></param>
            public void AdvanceTo(int targetTime)
                {
                if (targetTime <= time)
                    throw new ArgumentOutOfRangeException("targetTime", "Can only advance time forwards. Travelling backwards in time could create a time paradox!");
                var timesToRemove = new List<int>();
                foreach (var entry in actions.TakeWhile(e => e.Key <= targetTime))
                    {
                    timesToRemove.Add(entry.Key);
                    entry.Value();
                    context.PumpAll();
                    }
                foreach (int key in timesToRemove)
                    actions.Remove(key);
                time = targetTime;
                }

            /// <summary>
            ///   Advances the clock by the given number of arbitrary time units
            /// </summary>
            /// <param name="amount"></param>
            public void AdvanceBy(int amount)
                {
                if (amount <= 0)
                    throw new ArgumentOutOfRangeException("amount", "Can only advance time forwards");
                AdvanceTo(time + amount);
                }

            /// <summary>
            ///   Advances the clock by one time unit.
            /// </summary>
            public void Advance()
                {
                AdvanceBy(1);
                }
            }
        }
    }
