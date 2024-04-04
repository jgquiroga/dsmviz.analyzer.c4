using Dsmviz.Analyzer.C4.Settings;
using Dsmviz.Analyzer.C4.Test.Utils;
using Dsmviz.Datamodel.Dsi.Core;
using Dsmviz.Datamodel.Dsi.Interface;
using System.Reflection;

namespace Dsmviz.Analyzer.C4.Test.Analysis
{
    public class AnalyzerTest
    {
        [Fact]
        public void TestAnalyze()
        {
            AnalyzerSettings analyzerSettings = AnalyzerSettings.CreateDefault();
            analyzerSettings.Input.Workspace = Path.Combine(TestData.RootDirectory, TestData.TestWorkspace1);
            analyzerSettings.Transformation.IgnoredNames = new List<string>();

            IDsiModel model = new DsiModel("Test", analyzerSettings.Transformation.IgnoredNames, Assembly.GetExecutingAssembly());
            C4.Analysis.Analyzer analyzer = new C4.Analysis.Analyzer(model, analyzerSettings, null);

            analyzer.Analyze();

            // Main elements
            var elementUser = model.FindElementByName("User");
            Assert.NotNull(elementUser);

            var elementAdmin = model.FindElementByName("Admin");
            Assert.NotNull(elementAdmin);

            var elementSoftwareSystem = model.FindElementByName("Software System");
            Assert.NotNull(elementSoftwareSystem);

            var elementSoftwareSystemApp1 = model.FindElementByName("Software System.Web Application 1");
            Assert.NotNull(elementSoftwareSystemApp1);

            var elementSoftwareSystemApp2 = model.FindElementByName("Software System.Web Application 2");
            Assert.NotNull(elementSoftwareSystemApp2);

            var elementSoftwareSystemApp3 = model.FindElementByName("Software System.Web Application 3");
            Assert.NotNull(elementSoftwareSystemApp3);

            var elementUsersController = model.FindElementByName("Software System.Web Application 2.Users Controller");
            Assert.NotNull(elementUsersController);

            var elementPermissionsController = model.FindElementByName("Software System.Web Application 2.Permissions Controller");
            Assert.NotNull(elementPermissionsController);

            var elementDatabase1 = model.FindElementByName("Software System.Database1");
            Assert.NotNull(elementDatabase1);

            var elementDatabase2 = model.FindElementByName("Software System.Database2");
            Assert.NotNull(elementDatabase2);

            var elementWebserver1 = model.FindElementByName("Deployments.Development.Web Server 1");
            Assert.NotNull(elementWebserver1);

            var elementContainerInstance1 = model.FindElementByName("Deployments.Development.Web Server 1.webapp1instance");
            Assert.NotNull(elementContainerInstance1);

            var elementContainerInstance2 = model.FindElementByName("Deployments.Development.Web Server 1.webapp2instance");
            Assert.NotNull(elementContainerInstance2);

            var elementWebserver2 = model.FindElementByName("Deployments.Development.Web Server 2");
            Assert.NotNull(elementWebserver2);

            var elementContainerInstance3 = model.FindElementByName("Deployments.Development.Web Server 2.webapp3instance");
            Assert.NotNull(elementContainerInstance3);

            var elementDatabaseServer = model.FindElementByName("Deployments.Development.Database Server");
            Assert.NotNull(elementDatabaseServer);

            var elementContainerInstance4 = model.FindElementByName("Deployments.Development.Web Server 2.Redis Server");
            Assert.NotNull(elementContainerInstance4);

            var elementContainerInstance5 = model.FindElementByName("Deployments.Development.Web Server 2.webapp3instance");
            Assert.NotNull(elementContainerInstance5);

            // Main relations
            Assert.True(model.DoesRelationExist(elementUser.Id, elementSoftwareSystem.Id));
            Assert.True(model.DoesRelationExist(elementUser.Id, elementSoftwareSystemApp1.Id));

        }
    }
}