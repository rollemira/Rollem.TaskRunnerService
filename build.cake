var target = Argument("target", "Default");
var configuration = "Release";
var solutionFile = "./TaskRunnerService.sln";
var buildDirectory = Directory("./build");
var projectBuildDirectories = GetDirectories(string.Format("./**/bin/{0}", configuration));

Task("Default")
    .IsDependentOn("Copy");

Task("Clean")
    .Does(()=>{
        //get things to clean
        var c = new List<DirectoryPath>();
        c.Add(buildDirectory);
        c.AddRange(projectBuildDirectories);
        //clean
        CleanDirectories(c);
        //make directories
        CreateDirectory(buildDirectory);
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

RunTarget(target);