string NuGetVersionV2 = "";
string SolutionFileName = "src/StoneAssemblies.Extensibility.sln";

string[] DockerFiles = System.Array.Empty<string>();

string[] OutputImages = System.Array.Empty<string>();

string[] ComponentProjects  = new [] {
	"./src/StoneAssemblies.Extensibility/StoneAssemblies.Extensibility.csproj"
};

// TODO: Improve
string TestProject = "src/StoneAssemblies.Extensibility.Tests/StoneAssemblies.Extensibility.Tests.csproj";
string CoverageFilePath = System.IO.Path.GetFullPath("src/StoneAssemblies.Extensibility.Tests/coverage.opencover.xml");
string TestResultFilePath = System.IO.Path.GetFullPath("src/StoneAssemblies.Extensibility.Tests/TestResults.trx");

string SonarProjectKey = "stoneassemblies_StoneAssemblies.Extensibility";
string SonarOrganization = "stoneassemblies";