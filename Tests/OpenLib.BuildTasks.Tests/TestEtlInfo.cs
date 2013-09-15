using NUnit.Framework;

namespace OpenLib.BuildTasks.Tests
{
    [TestFixture]
    public class TestEtlInfo
    {
        private const string EtlInfoPath = @"Tasks\EtlInfo\{0}";

        private EtlInfo task;

        [SetUp]
        public void SetUp()
        {
            task = new EtlInfo { ProjectDir = string.Empty };
        }

        private string Get(string path)
        {
            return string.Format(EtlInfoPath, path);
        }

        [Test]
        public void TestExcecutionDoesNotObtainVersionWhenProjectDirIsNull()
        {
            // setup
            // see SetUp

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExcecutionObtainsEtlInfoWithVersion()
        {
            // setup
            task.EtlInfoPath = this.Get("Version.txt");

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
            task.EtlInfoPath = this.Get("SemanticVersion.txt");

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
