using System.Linq;

var target = Argument("target", "Default");
var configuration = "Release";
var solutionFile = "./TaskRunnerService.sln";
var buildDirectory = Directory("./build");
var projectOutputDirectory = Directory("./Rollem.TaskRunnerService/bin/" + configuration);
var publishDirectory = Directory("./publish");
var publishPath = publishDirectory.Path + "/RollemTaskRunnerService.zip";

Task("Default")
    .IsDependentOn("Publish");

Task("Clean")
    .Does(()=>{
        //get things to clean
        var c = new List<DirectoryPath>();
        c.Add(buildDirectory);
        c.Add(publishDirectory);
        c.Add(projectOutputDirectory);
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
        CopyDirectory(projectOutputDirectory, buildDirectory);
        DeleteFiles(buildDirectory.Path + "/**/*.xml");
        DeleteFiles(buildDirectory.Path + "/**/*.pdb");
    });

Task("Publish")
    .IsDependentOn("Copy")
    .Does(()=>{
        Zip(buildDirectory, publishPath);
    });

RunTarget(target);