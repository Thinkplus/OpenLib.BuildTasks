using NUnit.Framework;

namespace OpenLib.BuildTasks.Tests
{
    [TestFixture]
    public class TestEtlInfo
    {
        private const string InfoPath = @"Tasks\EtlInfo\{0}";

        private EtlInfo task;

        [SetUp]
        public void SetUp()
        {
            task = new EtlInfo { ProjectDir = string.Empty };
        }

        /// <summary>
        /// Gets the absolute path for the specified relative path.
        /// </summary>
        /// <param name="path">The relative path.</param>
        /// <returns>An absolute path from the relative path.</returns>
        private string Get(string path)
        {
            return string.Format(InfoPath, path);
        }

        [Test]
        public void TestExcecutionDoesNotObtainEtlInfoWhenProjectDirIsNull()
        {
            // setup
            // N/A

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExcecutionObtainsEtlInfoWithVersion()
        {
            // setup
            task.InfoPath = this.Get("Version.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("MyETL", task.Title);
            Assert.AreEqual("My ETL Package", task.Description);
            Assert.AreEqual("Chris Jaehnen (CJ)", task.Company);
            Assert.AreEqual("1.0.0.0", task.Version);
        }

        [Test]
        public void TestExcecutionObtainsEtlInfoWithSemanticVersion()
        {
            // setup
            task.InfoPath = this.Get("SemanticVersion.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("MyETL", task.Title);
            Assert.AreEqual("My ETL Package", task.Description);
            Assert.AreEqual("Chris Jaehnen (CJ)", task.Company);
            Assert.AreEqual("1.0.0-d", task.Version);
        }
    }
}
