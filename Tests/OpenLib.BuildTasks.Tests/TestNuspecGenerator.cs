using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;
using OpenLib.Utils;
using System.IO;
using TestUtilities;

namespace OpenLib.BuildTasks.Tests
{
    [TestFixture]
    public class TestNuspecGenerator
    {
        private const string ProjectDir = @"Tasks\NuspecGenerator\";
        private const string Assembly = "OpenLib.BuildTasks.dll";
        private const string Configuration = "Debug";

        private FileTestHelper fileTestHelper;
        private CodeInfoUtils codeInfoUtils;
        private NuspecGenerator task;

        [SetUp]
        public void SetUp()
        {
            fileTestHelper = new FileTestHelper();
            codeInfoUtils = new CodeInfoUtils();
            task = new NuspecGenerator
            {
                PackageDir = string.Empty,
                ProjectDir = ProjectDir,
                OutputPath = Path.GetFullPath(Assembly),
                Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.CSharp),
                Configuration = Configuration
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (task.NuspecFile != null && fileTestHelper.FileExists(task.NuspecFile))
            {
                fileTestHelper.DeleteFile(task.NuspecFile);
            }
        }
        
        [Test]
        public void TestExecutionDoesNotGenerateNuspecFileWhenPackageDirectoryIsNull()
        {
            // setup
            task.PackageDir = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotGenerateNuspecFileWhenProjectDirectoryIsNull()
        {
            // setup
            task.ProjectDir = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotGenerateNuspecFileWhenOutputPathIsNull()
        {
            // setup
            task.OutputPath = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotGenerateNuspecFileWhenOutputPathIsEmpty()
        {
            // setup
            task.OutputPath = string.Empty;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotGenerateNuspecFileWhenLanguageIsNull()
        {
            // setup
            task.Language = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotGenerateNuspecFileWhenLanguageIsEmpty()
        {
            // setup
            task.Language = string.Empty;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileForAssemblyVersion()
        {
            // setup
            // see SetUp

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(fileTestHelper.FileExists(task.NuspecFile));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileForAssemblyVersionWithoutConfiguation()
        {
            // setup
            task.Configuration = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(fileTestHelper.FileExists(task.NuspecFile));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileForAssemblySemanticVersion()
        {
            // setup
            task.ProjectDir += "Semantic";

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(fileTestHelper.FileExists(task.NuspecFile));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileForAssemblySemanticVersionWithoutConfiguation()
        {
            // setup
            task.ProjectDir += "Semantic";
            task.Configuration = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(fileTestHelper.FileExists(task.NuspecFile));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileForTSql()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.TSql);

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(fileTestHelper.FileExists(task.NuspecFile));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileForTSqlWithoutConfiguration()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.TSql);
            task.Configuration = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(fileTestHelper.FileExists(task.NuspecFile));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileForEtl()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.Etl);

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(fileTestHelper.FileExists(task.NuspecFile));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileForEtlWithoutConfiguration()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.Etl);
            task.Configuration = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(fileTestHelper.FileExists(task.NuspecFile));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileForCobol()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.Cobol);

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(fileTestHelper.FileExists(task.NuspecFile));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileForCobolWithoutConfiguration()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.Cobol);
            task.Configuration = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(fileTestHelper.FileExists(task.NuspecFile));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileWithDependencies()
        {
            // setup
            ITaskItem dependency1 = new TaskItem { ItemSpec = @"Dependency1" };
            dependency1.SetMetadata("Version", "1.0.0");
            dependency1.SetMetadata("TargetFramework", "net451");

            ITaskItem dependency2 = new TaskItem { ItemSpec = @"Dependency2" };
            dependency2.SetMetadata("Version", "1.0.0");
            dependency2.SetMetadata("TargetFramework", "net451");

            ITaskItem dependency3 = new TaskItem { ItemSpec = @"Dependency3" };
            dependency3.SetMetadata("Version", "1.0.0");

            ITaskItem[] dependencies = { dependency1, dependency2, dependency3 };

            task.Dependencies = dependencies;

            // execute
            bool result = task.Execute();

            string contents = fileTestHelper.ReadFile(task.NuspecFile);

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(contents.Contains(dependency1.ItemSpec));
            Assert.IsTrue(contents.Contains(dependency2.ItemSpec));
            Assert.IsTrue(contents.Contains(dependency3.ItemSpec));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileWithDefaultFiles()
        {
            // setup
            ITaskItem libFile = new TaskItem { ItemSpec = @"*\*.dll" };

            // execute
            bool result = task.Execute();

            string contents = fileTestHelper.ReadFile(task.NuspecFile);

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(contents.Contains(libFile.ItemSpec));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileWithCustomFiles()
        {
            // setup
            ITaskItem libFile = new TaskItem { ItemSpec = @"*\*.dll" };

            ITaskItem jsFile = new TaskItem { ItemSpec = @"*\*.js" };
            jsFile.SetMetadata("Target", "content");

            ITaskItem cssFile = new TaskItem { ItemSpec = @"*\*.css" };
            cssFile.SetMetadata("Target", "content");

            ITaskItem[] files = { jsFile, cssFile };

            task.CustomFiles = files;

            // execute
            bool result = task.Execute();

            string contents = fileTestHelper.ReadFile(task.NuspecFile);

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(contents.Contains(libFile.ItemSpec));
            Assert.IsTrue(contents.Contains(jsFile.ItemSpec));
            Assert.IsTrue(contents.Contains(cssFile.ItemSpec));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileWithCustomFilesOnly()
        {
            // setup
            ITaskItem libFile = new TaskItem { ItemSpec = @"*\*.dll" };

            ITaskItem jsFile = new TaskItem { ItemSpec = @"*\*.js" };
            jsFile.SetMetadata("Target", "content");

            ITaskItem cssFile = new TaskItem { ItemSpec = @"*\*.css" };
            cssFile.SetMetadata("Target", "content");

            ITaskItem[] files = { jsFile, cssFile };

            task.ExcludeDefaultFiles = true;
            task.CustomFiles = files;

            // execute
            bool result = task.Execute();

            string contents = fileTestHelper.ReadFile(task.NuspecFile);

            // assert
            Assert.IsTrue(result);
            Assert.IsFalse(contents.Contains(libFile.ItemSpec));
            Assert.IsTrue(contents.Contains(jsFile.ItemSpec));
            Assert.IsTrue(contents.Contains(cssFile.ItemSpec));
        }

        [Test]
        public void TestExecutionGeneratesNuspecFileWithCustomFilesAndDirectory()
        {
            // setup
            ITaskItem dirFile = new TaskItem { ItemSpec = @"Test\" };
            dirFile.SetMetadata("Target", "content");

            ITaskItem jsFile = new TaskItem { ItemSpec = @"*\*.js" };
            jsFile.SetMetadata("Target", "content");

            ITaskItem cssFile = new TaskItem { ItemSpec = @"*\*.css" };
            cssFile.SetMetadata("Target", "content");

            ITaskItem[] files = { dirFile, jsFile, cssFile };

            task.CustomFiles = files;

            // execute
            bool result = task.Execute();

            string contents = fileTestHelper.ReadFile(task.NuspecFile);

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(contents.Contains(@"content\Test"));
            Assert.IsTrue(contents.Contains(jsFile.ItemSpec));
            Assert.IsTrue(contents.Contains(cssFile.ItemSpec));
        }
    }
}
