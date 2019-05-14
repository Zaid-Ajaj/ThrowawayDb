using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tooling.ProcessTasks;

[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    static AbsolutePath Source => RootDirectory / "src";
    static AbsolutePath ThrowawayDb => Source / "ThrowawayDb"; 
    static AbsolutePath Tests => RootDirectory / "tests";
    static AbsolutePath PublishDir => RootDirectory / "publish";
    static readonly string DOTNET = "dotnet";
    public static int Main () => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target Clean => task => 
      task  
        .Executes(() =>
        {
            var directories = new List<string> {
                ThrowawayDb / "bin",
                ThrowawayDb / "obj", 
                Tests / "bin",
                Tests / "obj",
                PublishDir
            };

            foreach(var dir in directories) {
                DeleteDirectory(dir);
            }
        });

    Target Compile => task =>
      task
        .DependsOn(Clean)
        .Executes(() => {
            StartProcess(DOTNET, "restore --no-cache", ThrowawayDb).AssertZeroExitCode();
            StartProcess(DOTNET, "build", ThrowawayDb).AssertZeroExitCode();
            StartProcess(DOTNET, "restore --no-cache", Tests).AssertZeroExitCode();
            StartProcess(DOTNET, "build", Tests).AssertZeroExitCode();
        });

    Target Test => task => 
      task
        .DependsOn(Compile)
        .Executes(() =>
        {
            StartProcess(DOTNET, "restore", Tests).AssertZeroExitCode();
            StartProcess(DOTNET, "build", Tests).AssertZeroExitCode();
            StartProcess(DOTNET, "run", Tests).AssertZeroExitCode();
        });

    Target Pack => task =>
      task 
        .DependsOn(Compile)
        .Executes(() => 
        {
            var packCmd = $"pack -c Release -o {PublishDir}";
            StartProcess(DOTNET, packCmd, ThrowawayDb).AssertZeroExitCode();
        });

    Target Publish => task =>
      task
        .DependsOn(Pack) 
        .Executes(() => 
        {
            var nugetFile = Directory.GetFiles(PublishDir).FirstOrDefault() ?? "";
            if (!nugetFile.EndsWith(".nupkg"))
            {
                Logger.Error("No nuget package found");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            Logger.Info($"About to publish nuget package: {nugetFile}");
            var nugetApiKey = EnsureVariable("NUGET_KEY") ?? "";

            if (string.IsNullOrWhiteSpace(nugetApiKey))
            {
                Logger.Error("Nuget API Key was not setup on your local machine, missing environment variable NUGET_KEY");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            var nugetFileName = new FileInfo(nugetFile).Name;
            StartProcess(DOTNET, $"nuget push {nugetFileName} -s https://api.nuget.org/v3/index.json -k {nugetApiKey}", PublishDir).AssertZeroExitCode();
        });
}
