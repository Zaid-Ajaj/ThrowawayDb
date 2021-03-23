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
    static AbsolutePath ThrowawayDbPostgres => Source / "ThrowawayDb.Postgres";
    static AbsolutePath ThrowawayDbMySql => Source / "ThrowawayDb.MySql";
    static AbsolutePath ThrowawayDbTests => RootDirectory / "tests" / "ThrowawayDb.SqlServer";
    static AbsolutePath ThrowawayDbPostgresTests => RootDirectory / "tests" / "ThrowawayDb.Postgres";
    static AbsolutePath ThrowawayDbMySqlTests => RootDirectory / "tests" / "ThrowawayDb.MySql";
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
                ThrowawayDbPostgres / "bin",
                ThrowawayDbPostgres / "obj",
                ThrowawayDbTests / "bin",
                ThrowawayDbTests / "obj",
                ThrowawayDbPostgresTests / "bin",
                ThrowawayDbPostgresTests / "obj",
                PublishDir
            };

            foreach(var dir in directories)
            {
                DeleteDirectory(dir);
            }
        });

    Target Compile => task =>
      task
        .DependsOn(Clean)
        .Executes(() => {
            StartProcess(DOTNET, "restore --no-cache", ThrowawayDb).AssertZeroExitCode();
            StartProcess(DOTNET, "build", ThrowawayDb).AssertZeroExitCode();
            StartProcess(DOTNET, "restore --no-cache", ThrowawayDbPostgres).AssertZeroExitCode();
            StartProcess(DOTNET, "build", ThrowawayDbPostgres).AssertZeroExitCode();
            StartProcess(DOTNET, "restore --no-cache", ThrowawayDbMySql).AssertZeroExitCode();
            StartProcess(DOTNET, "build", ThrowawayDbMySql).AssertZeroExitCode();
            StartProcess(DOTNET, "restore --no-cache", ThrowawayDbTests).AssertZeroExitCode();
            StartProcess(DOTNET, "build", ThrowawayDbTests).AssertZeroExitCode();
            StartProcess(DOTNET, "restore --no-cache", ThrowawayDbPostgresTests).AssertZeroExitCode();
            StartProcess(DOTNET, "build", ThrowawayDbPostgresTests).AssertZeroExitCode();
            StartProcess(DOTNET, "restore --no-cache", ThrowawayDbMySqlTests).AssertZeroExitCode();
            StartProcess(DOTNET, "build", ThrowawayDbMySqlTests).AssertZeroExitCode();
        });

    Target Test => task =>
      task
        .DependsOn(Compile)
        .Executes(() =>
        {
            StartProcess(DOTNET, "restore", ThrowawayDbTests).AssertZeroExitCode();
            StartProcess(DOTNET, "build", ThrowawayDbTests).AssertZeroExitCode();
            StartProcess(DOTNET, "run", ThrowawayDbTests).AssertZeroExitCode();

            StartProcess(DOTNET, "restore", ThrowawayDbPostgresTests).AssertZeroExitCode();
            StartProcess(DOTNET, "build", ThrowawayDbPostgresTests).AssertZeroExitCode();
            StartProcess(DOTNET, "run", ThrowawayDbPostgresTests).AssertZeroExitCode();
        });

    Target PackThrowawayDb => task =>
      task
        .DependsOn(Compile)
        .Executes(() =>
        {
            var packCmd = $"pack -c Release -o {PublishDir}";
            StartProcess(DOTNET, packCmd, ThrowawayDb).AssertZeroExitCode();
        });

    Target PublishThrowawayDb => task =>
      task
        .DependsOn(PackThrowawayDb)
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

    Target PackThrowawayDbPostgres => task =>
      task
        .DependsOn(Compile)
        .Executes(() =>
        {
            var packCmd = $"pack -c Release -o {PublishDir}";
            StartProcess(DOTNET, packCmd, ThrowawayDbPostgres).AssertZeroExitCode();
        });

    Target PackThrowawayDbMySql => task =>
      task
        .DependsOn(Compile)
        .Executes(() =>
        {
            var packCmd = $"pack -c Release -o {PublishDir}";
            StartProcess(DOTNET, packCmd, ThrowawayDbMySql).AssertZeroExitCode();
        });

    Target PublishThrowawayDbPostgres => task =>
      task
        .DependsOn(PackThrowawayDbPostgres)
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

    Target PublishThrowawayDbMySql => task =>
      task
        .DependsOn(PackThrowawayDbMySql)
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
