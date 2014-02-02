﻿using NUnit.Framework;

namespace OpenLib.BuildTasks.Tests
{
    [TestFixture]
    public class TestDbInfo
    {
        private const string InfoPath = @"Tasks\DbInfo\{0}";

        private DbInfo task;

        [SetUp]
        public void SetUp()
        {
            task = new DbInfo { ProjectDir = string.Empty };
        }

        private string Get(string path)
        {
            return string.Format(InfoPath, path);
        }

        [Test]
        public void TestExecutionDoesNotObtainDbInfoWhenProjectDirIsNull()
        {
            // setup
            // see SetUp

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionObtainsDbInfoWithVersion()
        {
            // setup
            task.InfoPath = this.Get("Version.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("MyDatabase", task.Title);
            Assert.AreEqual("My Data Store", task.Description);
            Assert.AreEqual("Chris Jaehnen (CJ)", task.Company);
            Assert.AreEqual("1.0.0.0", task.Version);
        }

        [Test]
        public void TestExecutionObtainsDbInfoWithSemanticVersion()
        {
            // setup
            task.InfoPath = this.Get("SemanticVersion.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("MyDatabase", task.Title);
            Assert.AreEqual("My Data Store", task.Description);
            Assert.AreEqual("Chris Jaehnen (CJ)", task.Company);
            Assert.AreEqual("1.0.0-d", task.Version);
        }
    }
}
