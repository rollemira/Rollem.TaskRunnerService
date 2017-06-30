var target = Argument("target", "Default");
var configuration = "Release";
var solutionFile = "./TaskRunnerService.sln";
var output = Directory("./build");
var projectBuild = GetDirectories(string.Format("./**/bin/{0}", configuration));
var zipName = "RollemTaskRunnerService.zip";
var publishPath = "./binaries/" + zipName;

Task("Default")
    .IsDependentOn("Publish");

Task("Clean")
    .Does(()=>{
        var c = new List<DirectoryPath>();
        c.Add(output);
        c.AddRange(projectBuild);
        CleanDirectories(c);
    });

Task("RestorePackages")
    .IsDependentOn("Clean")
    .Does(()=>{
        NuGetRestore(solutionFile);
    });

Task("Build")
    .IsDependentOn("RestorePackages")
    .Does(()=>{
        MSBuild(solutionFile, new MSBuildSettings {
            Configuration = configuration
        });
    });

Task("Copy")
    .IsDependentOn("Build")
    .Does(()=>{
        foreach(var d in projectBuild) {
            CopyDirectory(d, output);
        }
    });

Task("Publish")
    .IsDependentOn("Copy")
    .Does(()=>{
        Zip(output, publishPath);
    });

RunTarget(target);