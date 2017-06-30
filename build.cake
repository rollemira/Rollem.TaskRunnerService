var target = Argument("target", "Default");
var configuration = "Release";
var solutionFile = "./TaskRunnerService.sln";
var buildDirectory = Directory("./build");
var publishDirectory = Directory("./publish");
var projectBuildDirectories = GetDirectories(string.Format("./**/bin/{0}", configuration));
var zipName = "RollemTaskRunnerService.zip";
var publishPath = "./publish/" + zipName;

Task("Default")
    .IsDependentOn("Publish");

Task("Clean")
    .Does(()=>{
        //get things to clean
        var c = new List<DirectoryPath>();
        c.Add(buildDirectory);
        c.Add(publishDirectory);
        c.AddRange(projectBuildDirectories);
        //clean
        CleanDirectories(c);
        //make directories
        CreateDirectory(buildDirectory);
        CreateDirectory(publishDirectory);
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
        foreach(var d in projectBuildDirectories) {
            CopyDirectory(d, buildDirectory);
        }
    });

Task("Publish")
    .IsDependentOn("Copy")
    .Does(()=>{
        Zip(buildDirectory, publishPath);
    });

RunTarget(target);