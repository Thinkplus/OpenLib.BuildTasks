using NUnit.Framework;
using OpenLib.Utils;

namespace OpenLib.BuildTasks.Tests
{
    [TestFixture]
    public class TestVersioning
    {
        private const string VersionInfoPath = @"Tasks\Versioning\{0}\{1}";

        private CodeInfoUtils codeInfoUtils;
        private Versioning task;

        [SetUp]
        public void SetUp()
        {
            codeInfoUtils = new CodeInfoUtils();

            task = new Versioning
            {
                ProjectDir = string.Empty,
                Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.CSharp),
                VersionPart = "1"
            };
        }

        /// <summary>
        /// Gets the absolute path for the specified relative path using the
        /// specified <see cref="CodeLanguage"/>.
        /// </summary>
        /// <param name="path">The relative path.</param>
        /// <returns>An absolute path from the relative path and <see cref="CodeLanguage"/>.</returns>
        private string Get(CodeLanguage codeLang, string path)
        {
            return string.Format(VersionInfoPath, codeLang.ToString(), path);
        }

        [Test]
        public void TestExecutionSetsVersionInfoPathForCSharp()
        {
            // setup
            string expected = codeInfoUtils.GetCodeVersionFile(CodeLanguage.CSharp);

            // execute
            task.Execute();

            // assert
            Assert.AreEqual(expected, task.VersionInfoPath);
        }

        [Test]
        public void TestExecutionSetsVersionInfoPathForVisualBasic()
        {
            // setup
            string expected = codeInfoUtils.GetCodeVersionFile(CodeLanguage.VisualBasic);

            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.VisualBasic);

            // execute
            task.Execute();

            // assert
            Assert.AreEqual(expected, task.VersionInfoPath);
        }

        [Test]
        public void TestExecutionSetsVersionInfoPathForTSql()
        {
            // setup
            string expected = codeInfoUtils.GetCodeVersionFile(CodeLanguage.TSql);

            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.TSql);

            // execute
            task.Execute();

            // assert
            Assert.AreEqual(expected, task.VersionInfoPath);
        }

        [Test]
        public void TestExecutionSetsVersionInfoPathForEtl()
        {
            // setup
            string expected = codeInfoUtils.GetCodeVersionFile(CodeLanguage.Etl);

            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.Etl);

            // execute
            task.Execute();

            // assert
            Assert.AreEqual(expected, task.VersionInfoPath);
        }

        [Test]
        public void TestExecutionSetsVersionInfoPathForCobol()
        {
            // setup
            string expected = codeInfoUtils.GetCodeVersionFile(CodeLanguage.Cobol);

            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.Cobol);

            // execute
            task.Execute();

            // assert
            Assert.AreEqual(expected, task.VersionInfoPath);
        }

        [Test]
        public void TestExecutionDoesNotApplyVersionWhenProjectDirectoryIsNull()
        {
            // setup
            task.ProjectDir = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotApplyVersionWhenLanguageIsNull()
        {
            // setup
            task.Language = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotApplyVersionWhenVersionPartIsNull()
        {
            // setup
            task.VersionPart = null;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDoesNotApplyVersionWhenVersionInfoFileDoesNotExist()
        {
            // setup
            task.VersionInfoPath = "DoesNotExist";

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestExecutionDeterminesVersionIsNotSemantic()
        {
            // setup
            // see SetUp

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsFalse(task.IsSemanticVersion);
        }

        [Test]
        public void TestExecutionAppliesVersionForDynamicRevisionVersionForAssemblyVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblyVersion\Dynamic\Revision.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.0.1", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForFixedRevisionVersionForAssemblyVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblyVersion\Fixed\Revision.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.0.1", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForDynamicBuildVersionForAssemblyVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblyVersion\Dynamic\Build.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.1.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForFixedBuildVersionForAssemblyVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblyVersion\Fixed\Build.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.1.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForDynamicMinorVersionForAssemblyVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblyVersion\Dynamic\Minor.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.1.0.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForFixedMinorVersionForAssemblyVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblyVersion\Fixed\Minor.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.1.0.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForDynamicMajorVersionForAssemblyVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblyVersion\Dynamic\Major.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.0.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForFixedMajorVersionForAssemblyVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblyVersion\Fixed\Major.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.0.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForTSqlVersion()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.TSql);
            task.VersionInfoPath = this.Get(CodeLanguage.TSql, "Version.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.0.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForEtlVersion()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.Etl);
            task.VersionInfoPath = this.Get(CodeLanguage.Etl, "Version.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.0.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForCobolVersion()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.Cobol);
            task.VersionInfoPath = this.Get(CodeLanguage.Cobol, "Version.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.0.0", task.Version);
        }

        [Test]
        public void TestExecutionDeterminesVersionIsSemanticForAssembly()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\Dev.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(task.IsSemanticVersion);
        }

        [Test]
        public void TestExecutionAppliesVersionForDevelopmentVersionForAssemblySemanticVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\Dev.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(task.Version.Contains("1.0.1-d"));
        }

        [Test]
        public void TestExecutionAppliesVersionForRevisionVersionForAssemblySemanticVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\Revision.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.1", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForMinorVersionForAssemblySemanticVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\Minor.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.1.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForMajorVersionForAssemblySemanticVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\Major.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForDynamicVersionForAssemblySemanticVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\Dynamic.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.0", task.Version);
        }

        [Test]
        public void TestExecutionAppliesVersionForReleaseVersionForAssemblySemanticVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\Release.txt");
            task.IsRelease = true;

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(task.Version.Contains("1.0.1"));
        }

        [Test]
        public void TestExecutionAppliesSpecifiedVersionForReleaseVersionForAssemblySemanticVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\ReleaseSpecified.txt");
            task.IsRelease = true;
            task.ReleaseVersion = "1.1.0";

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(task.Version.Contains("1.1.0"));
        }

        [Test]
        public void TestExecutionAppliesVersionForNewDevelopmentVersionForAssemblySemanticVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\New.txt");
            task.IsNewDevelopmentVersion = true;
            task.NewDevelopmentVersion = "1.1.0-d";

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(task.Version.Contains("1.1.0-d"));
        }

        [Test]
        public void TestExecutionAppliesVersionForNewDevelopmentVersionWithSemanticVersionIndicatorForAssemblySemanticVersion()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\New.txt");
            task.IsNewDevelopmentVersion = true;
            task.NewDevelopmentVersion = "1.1.0";

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(task.Version.Contains("1.1.0-d"));
        }

        [Test]
        public void TestExecutionSetsNextReleaseAndNewDevelopmentVersions()
        {
            // setup
            task.VersionInfoPath = this.Get(CodeLanguage.CSharp, @"AssemblySemanticVersion\Next.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual("1.0.1", task.NextReleaseVersion);
            Assert.AreEqual("1.1.0-d", task.NextNewDevelopmentVersion);
        }

        [Test]
        public void TestExecutionAppliesVersionForTSqlVersionForSemanticVersion()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.TSql);
            task.VersionInfoPath = this.Get(CodeLanguage.TSql, "SemanticVersion.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(task.Version.Contains("1.0.0-d"));
        }

        [Test]
        public void TestExecutionAppliesVersionForEtlVersionForSemanticVersion()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.Etl);
            task.VersionInfoPath = this.Get(CodeLanguage.Etl, "SemanticVersion.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(task.Version.Contains("1.0.0-d"));
        }

        [Test]
        public void TestExecutionAppliesVersionForCobolVersionForSemanticVersion()
        {
            // setup
            task.Language = codeInfoUtils.GetCodeLanguage(CodeLanguage.Cobol);
            task.VersionInfoPath = this.Get(CodeLanguage.Cobol, "SemanticVersion.txt");

            // execute
            bool result = task.Execute();

            // assert
            Assert.IsTrue(result);
            Assert.IsTrue(task.Version.Contains("1.0.0-d"));
        }
    }
}
