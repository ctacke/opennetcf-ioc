using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.IoC;

namespace IoC.Unit.Test
{
    [TestClass]
    public class ServiceCollectionTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var wi = new WorkItem())
            {
                var startCount = wi.Services.Count; // this will be non-zero due to IoC infrastructure
                wi.Services.AddNewIfMissing<FooServiceA>();
                wi.Services.AddNewIfMissing<FooServiceB, IFoo>();

                Assert.AreEqual(startCount + 2, wi.Services.Count);

                wi.Services.AddNewIfMissing<FooServiceA>();
                wi.Services.AddNewIfMissing<FooServiceB, IFoo>();

                Assert.AreEqual(startCount + 2, wi.Services.Count);
            }
        }
    }

    public interface IFoo
    {
    }

    public class FooServiceA : IFoo
    {
    }

    public class FooServiceB : IFoo
    {
    }
}
