using NUnit.Framework;

namespace OpenLib.BuildTasks.Tests
{
    [TestFixture]
    public class TestSonarVersioning
    {
        private const string SolutionDir = @"Tasks\SonarVersioning\";

        private SonarVersioning task;

        [SetUp]
        public void SetUp()
        {
            task = new SonarVersioning
            {
                SolutionDir = this.Get(),
                Version = "1.0.0"
            };
        }

        /// <summary>
        /// Gets the absolute path for the specified relative path.
        /// </summary>
        /// <param name="path">The relative path.</param>
        /// <returns>An absolute path from the relative path.</returns>
        private string Get(string path = null)
        {
            return string.Concat(SolutionDir, path);
        }

        [Test]
        public void TestExecutionSetsOutputPathForSolution()
        {
            // setup
            string expected = this.Get(SonarVersioning.SonarProjectConfig);

            // execute
            task.Execute();

            // assert
            Assert.AreEqual(expected, task.OutputFilePath);
        }

        [Test]
        public void TestExecutionDoesNotSetOutputPathForSolutionWhenSolutionDirectoryIsNull()
        {
            // setup
            task.SolutionDir = null;

            // execute
            task.Execute();

            // assert
            Assert.IsNull(task.OutputFilePath);
        }

        [Test]
        public void TestExecutionSetsOutputPathForProject()
        {
            // setup
            string expected = this.Get(string.Format(@"Project\{0}", SonarVersioning.SonarProjectConfig));

            task.SolutionDir = null;
            task.ProjectDir = this.Get("Project");

            // execute
            task.Execute();

            // assert
            Assert.AreEqual(expected, task.OutputFilePath);
        }

        [Test]
        public void TestExecutionDoesNotSetOutputPathForProjectWhenProjectDirectoryIsNull()
        {
            // setup
            task.SolutionDir = null;
            task.ProjectDir = null;

            // execute
            task.Execute();

            // assert
            Assert.IsNull(task.OutputFilePath);
        }

        [Test]
        public void TestExecutionDoesNotRunWhenSolutionDirectoryIsNull()
        {
            // setup
            task.SolutionDir = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotRunWhenProjectDirectoryIsNull()
        {
            // setup
            task.SolutionDir = null;
            task.ProjectDir = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotRunWhenVersionIsNull()
        {
            // setup
            task.Version = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionUpdatesSonarProjectVersionForSolution()
        {
            // setup
            task.Version = "1.1.0";

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TestExecutionUpdatesSonarProjectVersionForProject()
        {
            // setup
            task.SolutionDir = null;
            task.ProjectDir = this.Get("Project");
            task.Version = "1.1.0";

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TestExecutionDoesNotUpdateSonarProjectVersionWhenNoConfigFileIsFound()
        {
            // setup
            task.SolutionDir = "DoesNotExist";
            task.Version = "1.1.0";

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }
    }
}
