using NUnit.Framework;

namespace OpenLib.BuildTasks.Tests
{
    [TestFixture]
    public class TestCobolInfo
    {
        private const string InfoPath = @"Tasks\CobolInfo\{0}";

        private CobolInfo task;

        [SetUp]
        public void SetUp()
        {
            task = new CobolInfo { ProjectDir = string.Empty };
        }

        private string Get(string path)
        {
            return string.Format(InfoPath, path);
        }

        [Test]
        public void TestExecutionDoesNotObtainVersionWhenProjectDirIsNull()
        {
            // setup
            // see SetUp

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionObtainsCobolInfoWithVersion()
        {
            // setup
            task.InfoPath = this.Get("Version.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("MyCOBOL", task.Title);
            Assert.AreEqual("My COBOL assembly", task.Description);
            Assert.AreEqual("Chris Jaehnen (CJ)", task.Company);
            Assert.AreEqual("1.0.0.0", task.Version);
        }

        [Test]
        public void TestExecutionObtainsCobolInfoWithSemanticVersion()
        {
            // setup
            task.InfoPath = this.Get("SemanticVersion.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("MyCOBOL", task.Title);
            Assert.AreEqual("My COBOL assembly", task.Description);
            Assert.AreEqual("Chris Jaehnen (CJ)", task.Company);
            Assert.AreEqual("1.0.0", task.Version);
        }
    }
}
