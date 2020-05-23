#addin nuget:?package=Cake.Incubator&version=5.1.0
#tool nuget:?package=NuGet.Commandline&version=5.5.1

var createExecsTask = Task("Create-Executables")
    .Does<BuildVersion>((buildVersion) =>
{
    var msBuildSettings = new DotNetCoreMSBuildSettings()
        .WithProperty("Version", buildVersion.SemVersion)
        .WithProperty("AssemblyVersion", buildVersion.MajorMinorPatch)
        .WithProperty("FileVersion", buildVersion.MajorMinorPatch)
        .WithProperty("AssemblyInformationalVersion", buildVersion.InformationalVersion)
        .WithProperty("PublishSingleFile", "true")
        .WithProperty("PublishTrimmed", "true");

    var project = ParseProject("./Source/Codecov/Codecov.csproj", configuration);
    var runtimes = project.NetCore.RuntimeIdentifiers;
    var output = (DirectoryPath)Directory("./artifacts/output");
    if (DirectoryExists(output)) {
        DeleteDirectory(output, new DeleteDirectorySettings {
            Recursive = true,
            Force = true,
        });
    }

    foreach (var runtime in runtimes) {
        var outputDir = output.Combine(runtime);

        DotNetCorePublish(project.ProjectFilePath.FullPath, new DotNetCorePublishSettings {
            Configuration   = configuration,
            Runtime         = runtime,
            SelfContained   = true,
            OutputDirectory = outputDir,
            MSBuildSettings = msBuildSettings,
        });
    }
});

var createDotNetToolTask = Task("Create-DotNetToolPackage")
    .IsDependentOn(buildTask)
    .Does<BuildVersion>((buildVersion) =>
{
    var msBuildSettings = new DotNetCoreMSBuildSettings()
        .WithProperty("Version", buildVersion.SemVersion)
        .WithProperty("AssemblyVersion", buildVersion.MajorMinorPatch)
        .WithProperty("FileVersion", buildVersion.MajorMinorPatch)
        .WithProperty("AssemblyInformationalVersion", buildVersion.InformationalVersion);

    var output = (DirectoryPath)Directory("./artifacts/packages");

    DotNetCorePack("./Source/Codecov.Tool", new DotNetCorePackSettings {
        Configuration = configuration,
        NoBuild = true,
        NoRestore = true,
        OutputDirectory = output,
        MSBuildSettings = msBuildSettings,
    });
});

var createArchives = Task("Create-Archives")
    .IsDependentOn(createExecsTask)
    .Does(() =>
{
    var directories = GetDirectories("./artifacts/output/*");
    CleanDirectory("./artifacts/archives");

    foreach (var directory in directories) {
        var output = $"./artifacts/archives/codecov-{directory.GetDirectoryName()}.zip";
        Zip(directory.FullPath, output);
    }
});

var createNuGetPackagesTask = Task("Create-NuGetPackages")
    .IsDependentOn(createExecsTask)
    .Does<BuildVersion>((buildVersion) =>
{
    var outputDirectory = "./artifacts/packages/nuget";
    var nuspecsBase = MakeAbsolute((DirectoryPath)"./nuspec/nuget");
    var nuspecFiles = GetFiles(nuspecsBase + "/*.nuspec");
    var basePath = MakeAbsolute((DirectoryPath)"./artifacts/output");

    NuGetPack(nuspecFiles, new NuGetPackSettings {
        Version                 = buildVersion.SemVersion,
        Symbols                 = true,
        BasePath                = basePath,
        OutputDirectory         = outputDirectory,
        ArgumentCustomization   = args=>args.AppendSwitch("-SymbolPackageFormat", "snupkg"),
    });
});

var createChocolateyPackagesTask = Task("Create-ChocolateyPackages")
    .WithCriteria(IsRunningOnWindows)
    .IsDependentOn(createArchives)
    .Does<BuildVersion>((buildVersion) =>
{
    var outputDirectory = "./artifacts/packages/chocolatey";
    var nuspecFiles = GetFiles("./nuspec/chocolatey/*.nuspec");
    EnsureDirectoryExists(outputDirectory);

    var files = (GetFiles("./nuspec/chocolatey/tools/*")
        + GetFiles("./artifacts/archives/*win7*.zip")
        + File("./LICENSE.txt")).Select((f) => new ChocolateyNuSpecContent { Source = MakeAbsolute(f).ToString(), Target = "tools" });

    ChocolateyPack(nuspecFiles, new ChocolateyPackSettings {
        Version = buildVersion.SemVersionPadded,
        OutputDirectory = outputDirectory,
        Files = files.ToList(),
    });
});

var createPackagesTask = Task("Create-Packages")
    .IsDependentOn(createDotNetToolTask)
    .IsDependentOn(createNuGetPackagesTask)
    .IsDependentOn(createChocolateyPackagesTask);
