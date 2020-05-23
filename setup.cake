#module "nuget:?package=Cake.DotNetTool.Module&version=0.4.0"
#load "./.build/*.cake"


var target = Argument("target", "Default");

testTask.IsDependentOn(buildTask);

Task("Default")
    .IsDependentOn(testTask)
    .IsDependentOn(createExecsTask)
    .IsDependentOn(createDotNetToolTask);

Task("AppVeyor")
    .IsDependentOn("Default")
    .IsDependentOn(coverageTask)
    .IsDependentOn(createPackagesTask);

RunTarget(target);
