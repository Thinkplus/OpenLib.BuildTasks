using NUnit.Framework;
using System.IO;

namespace OpenLib.BuildTasks.Tests
{
    [TestFixture]
    public class TestAssemblyInfo
    {
        private const string Assembly = "OpenLib.BuildTasks.Tests.dll";

        private AssemblyInfo task;

        [SetUp]
        public void SetUp()
        {
            task = new AssemblyInfo { AssemblyPath = Path.GetFullPath(Assembly) };
        }

        [Test]
        public void TestExecutionDoesNotObtainAssemblyInfoWhenAssemblyPathIsNull()
        {
            // setup
            task.AssemblyPath = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotObtainAssemblyInfoWhenAssemblyPathIsEmpty()
        {
            // setup
            task.AssemblyPath = string.Empty;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionObtainsAssemblyInfo()
        {
            // setup
            // see SetUp

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("OpenLib.BuildTasks.Tests", task.Title);
            Assert.AreEqual("OpenLib.BuildTasks.Tests unit and integration test library", task.Description);
            Assert.IsNullOrEmpty(task.Configuration);
            Assert.AreEqual("Chris Jaehnen (CJ)", task.Company);
            Assert.AreEqual("OpenLib.BuildTasks.Tests", task.Product);
            Assert.AreEqual("Copyright © CJ 2014", task.Copyright);
            Assert.IsNullOrEmpty(task.Trademark);
            Assert.IsNullOrEmpty(task.Culture);
            Assert.AreEqual("1.0.0", task.Version);
        }
    }
}
