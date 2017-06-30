using System.Linq;

var target = Argument("target", "Default");
var configuration = "Release";
var solutionFile = "./TaskRunnerService.sln";
var buildDirectory = Directory("./build");
var projectBuildDirectories = GetDirectories("./**/bin/" + configuration);
var publishDirectory = Directory("./publish");
var publishPath = publishDirectory.Path + "/RollemTaskRunnerService.zip";

Task("Default")
    .IsDependentOn("Publish");

Task("Clean")
    .Does(()=>{
        Information("Project build directories " + projectBuildDirectories.Count);
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
        CopyDirectory(projectBuildDirectories.First().FullPath, buildDirectory.Path);
        // foreach(var d in projectBuildDirectories) {
        //     CopyDirectory(d.FullPath, buildDirectory.Path);
        // }
    });

Task("Publish")
    .IsDependentOn("Copy")
    .Does(()=>{
        Zip(buildDirectory.Path, publishPath);
    });

RunTarget(target);