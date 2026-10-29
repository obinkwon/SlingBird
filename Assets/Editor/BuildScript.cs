using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 커맨드라인(fastlane) 빌드용. 예:
/// Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildAndroidAab
///   -buildPath build/android/SlingBird.aab -versionCode 123
/// 서명 정보는 환경변수로 받는다 (비밀번호를 프로젝트 파일에 남기지 않기 위해):
/// SLINGBIRD_KEYSTORE_PATH, SLINGBIRD_KEYSTORE_PASS, SLINGBIRD_KEY_ALIAS, SLINGBIRD_KEY_PASS
/// </summary>
public static class BuildScript
{
    private const string PackageName = "com.game.slingbird";

    public static void BuildAndroidAab()
    {
        string buildPath = GetArg("-buildPath") ?? "build/android/SlingBird.aab";
        string versionCode = GetArg("-versionCode");

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageName);
        if (versionCode != null)
            PlayerSettings.Android.bundleVersionCode = int.Parse(versionCode);

        // Google Play는 64비트 필수 → IL2CPP + ARM64
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = RequireEnv("SLINGBIRD_KEYSTORE_PATH");
        PlayerSettings.Android.keystorePass = RequireEnv("SLINGBIRD_KEYSTORE_PASS");
        PlayerSettings.Android.keyaliasName = RequireEnv("SLINGBIRD_KEY_ALIAS");
        PlayerSettings.Android.keyaliasPass = RequireEnv("SLINGBIRD_KEY_PASS");

        EditorUserBuildSettings.buildAppBundle = true;

        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = buildPath,
            target = BuildTarget.Android,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"Android 빌드 실패: {report.summary.result}");
            EditorApplication.Exit(1);
        }

        Debug.Log($"Android AAB 빌드 완료: {buildPath} (versionCode {PlayerSettings.Android.bundleVersionCode})");
    }

    private static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    private static string RequireEnv(string name)
    {
        string value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError($"환경변수 {name}가 없습니다. 서명 정보를 설정하세요.");
            EditorApplication.Exit(1);
        }
        return value;
    }
}
