#tool dotnet:?package=dotnet-reportgenerator-globaltool&version=4.5.8
#addin nuget:?package=Cake.Codecov&version=0.8.0

var testTask = Task("Test")
    .Does(() =>
{
    if (DirectoryExists("./Source/Codecov.Tests/TestResults")) {
        DeleteDirectory("./Source/Codecov.Tests/TestResults", new DeleteDirectorySettings {
            Recursive = true,
            Force = true,
        });
    }

    DotNetCoreTest("./Source/Codecov.Tests", new DotNetCoreTestSettings {
        Configuration = configuration,
        NoBuild = true,
        ArgumentCustomization = args=>args.Append("--collect:\"XPlat Code Coverage\""),
        Settings = File("./Source/Codecov.Tests/coverlet.runSettings"),
    });
});

var coverageGenTask = Task("Generate-LocalCoverage")
    .Does(() =>
{
    var files = GetFiles("./Source/*.Tests/TestResults/**/*.xml");
    var exePrefix = IsRunningOnWindows() ? ".exe" : "";
    var outputDir = "./artifacts/Coverage";
    if (DirectoryExists(outputDir)) {
        DeleteDirectory(outputDir, new DeleteDirectorySettings {
            Recursive = true,
            Force = true,
        });
    }

    ReportGenerator(files, outputDir, new ReportGeneratorSettings {
        ToolPath = $"./tools/reportgenerator{exePrefix}",
    });
});

var coverageTask = Task("Upload-CoverageReport")
    .Does(() =>
{
    var exePrefix = IsRunningOnWindows() ? ".exe" : "";

    Codecov(new CodecovSettings {
        Files = new[] { "Source/*.Tests/TestResults/**/*.xml"},
        ToolPath = File($"./Source/Codecov.Tool/bin/{configuration}/netcoreapp3.0/codecov{exePrefix}"),
        Verbose = true,
    });
});
