using System.Linq;
using System.Threading.Tasks;
using System;
using NUnit.Framework;
using System.Reactive.Linq;
using System.Collections.Generic;
using static StateStores.StateStoreResult;
using System.Reactive;

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
            const int EXPECTED_NEXT_NOTIFICATION_COUNT = 3;

            const int OBSERVER_DELAY_MS = 200;

            int mut_ActualAddNotificationCount = 0;
            int mut_ActualNextNotificationCount = 0;

            void SubscribeNextAndAdd()
            {
                channel.OnAdd.Subscribe(_ => mut_ActualAddNotificationCount++);
                channel.OnNext().Subscribe(_ => mut_ActualNextNotificationCount++);
            }

            SubscribeNextAndAdd();

            // Can set 
            AssertOk(await channel.AddAsync(SAMPLE_STATE_1));

            SubscribeNextAndAdd();
            SubscribeNextAndAdd();

            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(
                EXPECTED_ADD_NOTIFICATION_COUNT,
                mut_ActualAddNotificationCount);

            Assert.AreEqual(
                EXPECTED_NEXT_NOTIFICATION_COUNT,
                mut_ActualNextNotificationCount);
        }

        // This is a state-transition-chain where observers invoke transitions.
        public static async Task TestReactiveFunctionalityAsync(
            this IStateChannel<int> channel, int stepCount, int activeChannelCount = 1)
        {
            var actualStateHistory = new List<int>();
            var exptectedStateHistory = Enumerable.Range(0, stepCount).ToList();

            bool ShouldProceed(int i) => (i < stepCount);

            bool ShouldStop(int i) => !ShouldProceed(i);

            void LogState(int i) => actualStateHistory.Add(i);

            var tcsStop = new TaskCompletionSource<Unit>();

            channel // Logging
                .OnNext(ShouldProceed)
                .Do(LogState)
                .Subscribe();

            Enumerable // Procedure handlers
                .Range(0, activeChannelCount)
                .Select(_ =>
                    channel
                        .OnNextWithHandle(ShouldProceed)
                        .Update(i => i + 1)
                        .Subscribe())
                .ToList();

            channel // Termination
                .OnNextWithHandle(ShouldStop)
                .Remove()
                .Do(tcsStop.SetResult)
                .Subscribe();

            const int INITIAL_STATE = 0;
            AssertOk(await channel.AddAsync(INITIAL_STATE));

            await tcsStop.Task;
            await Task.Delay(100);

            Assert.IsTrue(
                condition: Enumerable.SequenceEqual(
                    exptectedStateHistory,
                    actualStateHistory),
                message: "Incorrect state history!"
            );
        }


    }
}