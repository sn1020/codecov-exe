#tool dotnet:?package=GitVersion.Tool&version=5.3.4

public class BuildVersion
{
    public string SemVersion { get; set; }
    public string SemVersionPadded { get; set; }
    public string MajorMinorPatch { get; set; }
    public string InformationalVersion { get; set; }
}

Setup<BuildVersion>((ctx) => {
        var gitVersion = ctx.GitVersion();

        ctx.Information($"Building Codecove.exe version {gitVersion.FullSemVer}!");

        return new BuildVersion
        {
            MajorMinorPatch      = gitVersion.MajorMinorPatch,
            SemVersion           = gitVersion.FullSemVer,
            SemVersionPadded     = gitVersion.LegacySemVerPadded,
            InformationalVersion = gitVersion.InformationalVersion,
        };
});
