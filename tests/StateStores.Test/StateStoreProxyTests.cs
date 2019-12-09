﻿using System.Linq;
using System.Threading.Tasks;
using System;
using NUnit.Framework;
using System.Reactive.Linq;
using System.Collections.Generic;
using static StateStores.StateStoreResult;


namespace StateStores.Test
{
    public static class StateChannelTests
    {
        static void AssertOk(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(Ok), result);

        public static async Task TestBasicFunctionalityAsync(this IStateChannel<string> channel)
        {
            const string SAMPLE_STATE_1 = "state1";
            const string SAMPLE_STATE_2 = "state2";

            const int EXPECTED_ADD_NOTIFICATION_COUNT = 1;
            const int EXPECTED_UPDATE_NOTIFICATION_COUNT = 1;
            const int EXPECTED_REMOVE_NOTIFICATION_COUNT = 1;
            const int EXPECTED_NEXT_NOTIFICATION_COUNT = 2;
            const int EXPECTED_PREVIOUS_NOTIFICATION_COUNT = 2;

            int mut_ActualAddNotificationCount = 0;
            int mut_ActualUpdateNotificationCount = 0;
            int mut_ActualRemoveNotificationCount = 0;
            int mut_ActualNextNotificationCount = 0;
            int mut_ActualPreviousNotificationCount = 0;

            channel.OnAdd
                .Subscribe(_ => mut_ActualAddNotificationCount += 1);
            channel.OnUpdate
                .Subscribe(_ => mut_ActualUpdateNotificationCount += 1);
            channel.OnRemove
                .Subscribe(_ => mut_ActualRemoveNotificationCount += 1);
            channel.OnNext()
                .Subscribe(_ => mut_ActualNextNotificationCount += 1);
            channel.OnPrevious()
                .Subscribe(_ => mut_ActualPreviousNotificationCount += 1);

            const int OBSERVER_DELAY_MS = 200;

            // Can set 
            AssertOk(await channel.AddAsync(SAMPLE_STATE_1));
            await Task.Delay(OBSERVER_DELAY_MS);

            // Can update 
            AssertOk(await channel.UpdateAsync(SAMPLE_STATE_1, SAMPLE_STATE_2));
            await Task.Delay(OBSERVER_DELAY_MS);

            // Can remove 
            AssertOk(await channel.RemoveAsync(SAMPLE_STATE_2));
            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(
                EXPECTED_ADD_NOTIFICATION_COUNT,
                mut_ActualAddNotificationCount);

            Assert.AreEqual(
                EXPECTED_UPDATE_NOTIFICATION_COUNT,
                mut_ActualUpdateNotificationCount);

            Assert.AreEqual(
                EXPECTED_REMOVE_NOTIFICATION_COUNT,
                mut_ActualRemoveNotificationCount);

            Assert.AreEqual(
                EXPECTED_NEXT_NOTIFICATION_COUNT,
                mut_ActualNextNotificationCount);

            Assert.AreEqual(
                EXPECTED_PREVIOUS_NOTIFICATION_COUNT,
                mut_ActualPreviousNotificationCount);
        }

        public static async Task TestReplayFunctionalityAsync(this IStateChannel<string> channel)
        {
            const string SAMPLE_STATE_1 = "state1";

            const int EXPECTED_ADD_NOTIFICATION_COUNT = 3;

            const int OBSERVER_DELAY_MS = 200;

            int mut_ActualAddNotificationCount = 0;

            channel.OnAdd.Subscribe(_ => mut_ActualAddNotificationCount += 1);

            // Can set 
            AssertOk(await channel.AddAsync(SAMPLE_STATE_1));

            channel.OnAdd.Subscribe(_ => mut_ActualAddNotificationCount += 1);
            channel.OnAdd.Subscribe(_ => mut_ActualAddNotificationCount += 1);

            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(
                EXPECTED_ADD_NOTIFICATION_COUNT,
                mut_ActualAddNotificationCount);
        }

        // This is a state-transition-chain where observers invoke transitions.
        public static async Task TestReactiveFunctionalityAsync(
            this IStateChannel<int> channel, int stateCount, int parallelHandlers = 1)
        {

            var mut_actualStateHistory = new List<int>();
            var exptectedStateHistory = Enumerable.Range(0, stateCount);

            bool ShouldProceed(int i) => (i < stateCount);

            bool ShouldStop(int i) => !ShouldProceed(i);

            void LogState(int i) => mut_actualStateHistory.Add(i);

            var tcsStop = new TaskCompletionSource<int>();

            Enumerable
                .Range(0, parallelHandlers)
                .Select(_ =>
                    channel
                        .OnNext(ShouldProceed)
                        .Do(i => channel.UpdateAsync(i, i + 1))
                        .Subscribe())
                .ToList();

            channel
                .OnNext(ShouldProceed)
                .Do(LogState)
                .Subscribe();

            channel
                .OnNextWithHandl(ShouldStop)
                .Do(h => h.RemoveAsync())
                .Select(h => h.Value)
                .Subscribe(tcsStop.SetResult);

            const int INITIAL_STATE = 0;
            AssertOk(await channel.AddAsync(INITIAL_STATE));

            var finalState = await tcsStop.Task;

            Assert.IsTrue(
                condition: Enumerable.SequenceEqual(
                    exptectedStateHistory,
                    mut_actualStateHistory),
                message: "Incorrect state history!"
            );
        }


    }
}