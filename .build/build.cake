var configuration = Argument("configuration", "Release");

var buildTask = Task("Build")
    .Does<BuildVersion>((buildVersion) =>
{
    var msBuildSettings = new DotNetCoreMSBuildSettings()
        .WithProperty("Version", buildVersion.SemVersion)
        .WithProperty("AssemblyVersion", buildVersion.MajorMinorPatch)
        .WithProperty("FileVersion", buildVersion.MajorMinorPatch)
        .WithProperty("AssemblyInformationalVersion", buildVersion.InformationalVersion);

    DotNetCoreBuild("./Source/Codecov.sln", new DotNetCoreBuildSettings {
        Configuration = configuration,
        MSBuildSettings = msBuildSettings,
    });
});
