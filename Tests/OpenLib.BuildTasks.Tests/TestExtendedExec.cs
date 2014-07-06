using Microsoft.Build.Framework;
using Moq;
using NUnit.Framework;

namespace OpenLib.BuildTasks.Tests
{
    [TestFixture]
    public class TestExtendedExec
    {
        private const string ExecCommand = "nuget sources";

        private Mock<IBuildEngine> buildEngine;
        private ExtendedExec task;

        [SetUp]
        public void SetUp()
        {
            buildEngine = new Mock<IBuildEngine>();
            task = new ExtendedExec
            {
                BuildEngine = buildEngine.Object,
                Command = ExecCommand
            };
        }

        [Test]
        public void TestExecutionReturnsOutput()
        {
            // setup
            // N/A

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsNotNullOrEmpty(task.Output);
        }

        [Test]
        public void TestExecutionFailsWhenTextStringIsFoundInOutput()
        {
            // setup
            string textString = "nuget.org";

            task.FailExecIfOutputContainsText = textString;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotFailWhenTextStringIsNotFoundInOutput()
        {
            // setup
            string textString = "nuget.com";

            task.FailExecIfOutputContainsText = textString;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TestExecutionFailsWhenOutputIsNotReturnedForContainsText()
        {
            // setup
            string textString = "nuget.org";

            task.Command = "nuget config";
            task.FailExecIfOutputContainsText = textString;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionFailsWhenTextStringIsNotFoundInOutput()
        {
            // setup
            string textString = "nuget.com";

            task.FailExecIfOutputDoesNotContainText = textString;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotFailWhenTextStringIsFoundInOutput()
        {
            // setup
            string textString = "nuget.org";

            task.FailExecIfOutputDoesNotContainText = textString;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TestExecutionFailsWhenOutputIsNotReturnedForDoesNotContainText()
        {
            // setup
            string textString = "nuget.com";

            task.Command = "nuget config";
            task.FailExecIfOutputDoesNotContainText = textString;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }
    }
}
