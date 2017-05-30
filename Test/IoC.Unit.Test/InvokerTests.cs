using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.IoC;

namespace IoC.Unit.Test
{
    public class Publisher
    {
        [EventPublication("TestEvent", PublicationScope=PublicationScope.Global)]
        public event EventHandler OnEvent;

        public void RaiseEvent()
        {
            OnEvent.Invoke(this, EventArgs.Empty);
        }
    }

    public class Subscriber
    {
        public bool EventReceived { get; set; }

        [EventSubscription("TestEvent", ThreadOption.Caller)]
        public void Publisher_OnEvent(object sender, EventArgs e)
        {
            EventReceived = true;
        }
    }

    [TestClass]
    public class InvokerTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var pub = RootWorkItem.Items.AddNew<Publisher>();
            var sub = RootWorkItem.Items.AddNew<Subscriber>();

            pub.RaiseEvent();

            Assert.IsTrue(sub.EventReceived);

        }
    }
}
