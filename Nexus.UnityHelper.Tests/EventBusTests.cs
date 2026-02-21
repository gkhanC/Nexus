using Xunit;
using Nexus.UnityHelper.Communication;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Nexus.UnityHelper.Tests
{
    public class EventBusTests
    {
        public class TestEvent : INexusEvent
        {
            public int Value;
        }

        [Fact]
        public void SubscribeAndPublish_Works()
        {
            NexusEventBus.Clear();
            int result = 0;
            NexusEventBus.Subscribe<TestEvent>(e => result = e.Value);
            
            NexusEventBus.Publish(new TestEvent { Value = 42 });
            
            Assert.Equal(42, result);
        }

        [Fact]
        public void Unsubscribe_Works()
        {
            NexusEventBus.Clear();
            int result = 0;
            System.Action<TestEvent> action = e => result = e.Value;
            
            NexusEventBus.Subscribe(action);
            NexusEventBus.Unsubscribe(action);
            
            NexusEventBus.Publish(new TestEvent { Value = 42 });
            
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task MultiThreadedPublish_IsThreadSafe()
        {
            NexusEventBus.Clear();
            int count = 0;
            object lockObj = new object();
            
            NexusEventBus.Subscribe<TestEvent>(e => {
                lock(lockObj) count++;
            });

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => NexusEventBus.Publish(new TestEvent())));
            }

            await Task.WhenAll(tasks);
            
            Assert.Equal(100, count);
        }
    }
}
