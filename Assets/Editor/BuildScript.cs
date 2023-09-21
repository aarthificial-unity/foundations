using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Editor {
  public static class BuildScript {
    private static readonly string _eol = System.Environment.NewLine;

    public static void Build() {
      // Gather values from args
      var options = GetValidatedOptions();

      // Set version for this build
      PlayerSettings.bundleVersion = options["buildVersion"];
      PlayerSettings.macOS.buildNumber = options["buildVersion"];

      // Apply build target
      var buildTarget = Enum.Parse<BuildTarget>(options["buildTarget"]);

      if (buildTarget == BuildTarget.StandaloneOSX) {
        PlayerSettings.SetScriptingBackend(
          BuildTargetGroup.Standalone,
          ScriptingImplementation.Mono2x
        );
      }

      // Custom build
      Build(buildTarget, options["customBuildPath"]);
    }

    private static Dictionary<string, string> GetValidatedOptions() {
      ParseCommandLineArguments(out var validatedOptions);

      if (!validatedOptions.ContainsKey("projectPath")) {
        Console.WriteLine("Missing argument -projectPath");
        EditorApplication.Exit(110);
      }

      if (!validatedOptions.TryGetValue("buildTarget", out var buildTarget)) {
        Console.WriteLine("Missing argument -buildTarget");
        EditorApplication.Exit(120);
      }

      if (!Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty)) {
        Console.WriteLine(
          $"{buildTarget} is not a defined {nameof(BuildTarget)}"
        );
        EditorApplication.Exit(121);
      }

      if (!validatedOptions.ContainsKey("customBuildPath")) {
        Console.WriteLine("Missing argument -customBuildPath");
        EditorApplication.Exit(130);
      }

      const string defaultCustomBuildName = "TestBuild";
      if (!validatedOptions.TryGetValue(
          "customBuildName",
          out var customBuildName
        )) {
        Console.WriteLine(
          $"Missing argument -customBuildName, defaulting to {defaultCustomBuildName}."
        );
        validatedOptions.Add("customBuildName", defaultCustomBuildName);
      } else if (customBuildName == "") {
        Console.WriteLine(
          $"Invalid argument -customBuildName, defaulting to {defaultCustomBuildName}."
        );
        validatedOptions.Add("customBuildName", defaultCustomBuildName);
      }

      return validatedOptions;
    }

    private static void ParseCommandLineArguments(
      out Dictionary<string, string> providedArguments
    ) {
      providedArguments = new Dictionary<string, string>();
      var args = System.Environment.GetCommandLineArgs();

      Console.WriteLine(
        $"{_eol}"
        + $"###########################{_eol}"
        + $"#    Parsing settings     #{_eol}"
        + $"###########################{_eol}"
        + $"{_eol}"
      );

      // Extract flags with optional values
      for (int current = 0, next = 1;
        current < args.Length;
        current++, next++) {
        // Parse flag
        var isFlag = args[current].StartsWith("-");
        if (!isFlag) {
          continue;
        }
        var flag = args[current].TrimStart('-');

        // Parse optional value
        var flagHasValue = next < args.Length && !args[next].StartsWith("-");
        var value = flagHasValue ? args[next].TrimStart('-') : "";
        var displayValue = "\"" + value + "\"";

        // Assign
        Console.WriteLine($"Found flag \"{flag}\" with value {displayValue}.");
        providedArguments.Add(flag, value);
      }
    }

    private static void Build(BuildTarget buildTarget, string filePath) {
      var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
        .Select(s => s.path)
        .ToArray();

      var buildPlayerOptions = new BuildPlayerOptions {
        scenes = scenes,
        target = buildTarget,
        targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget),
        locationPathName = filePath,
        options = BuildOptions.Development,
      };

      var buildSummary = BuildPipeline.BuildPlayer(buildPlayerOptions).summary;
      ReportSummary(buildSummary);
      ExitWithResult(buildSummary.result);
    }

    private static void ReportSummary(BuildSummary summary) {
      Console.WriteLine(
        $"{_eol}"
        + $"###########################{_eol}"
        + $"#      Build results      #{_eol}"
        + $"###########################{_eol}"
        + $"{_eol}"
        + $"Duration: {summary.totalTime.ToString()}{_eol}"
        + $"Warnings: {summary.totalWarnings.ToString()}{_eol}"
        + $"Errors: {summary.totalErrors.ToString()}{_eol}"
        + $"Size: {summary.totalSize.ToString()} bytes{_eol}"
        + $"{_eol}"
      );
    }

    private static void ExitWithResult(BuildResult result) {
      switch (result) {
        case BuildResult.Succeeded:
          Console.WriteLine("Build succeeded!");
          EditorApplication.Exit(0);
          break;
        case BuildResult.Failed:
          Console.WriteLine("Build failed!");
          EditorApplication.Exit(101);
          break;
        case BuildResult.Cancelled:
          Console.WriteLine("Build cancelled!");
          EditorApplication.Exit(102);
          break;
        case BuildResult.Unknown:
        default:
          Console.WriteLine("Build result is unknown!");
          EditorApplication.Exit(103);
          break;
      }
    }
  }
}
