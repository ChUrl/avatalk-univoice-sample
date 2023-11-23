using System.Diagnostics;
using Codice.Client.Common;
using UnityEditor;
using UnityEditor.Android;
using AndroidDevice = UnityEngine.Android.AndroidDevice;

namespace Editor
{
    public class BuildScript
    {
        private const string ArtifactName = "UnivoicePoC";
        private static readonly string[] Levels = {"Assets/Scenes/UnivoicePoC.unity"};

        private static string GetPath(string target)
        {
            return EditorUtility.SaveFolderPanel("Choose Build Location", $"Build_{target}", "");
        }

        [MenuItem("MyBuilds/0 - Build All")]
        public static void BuildAll()
        {
            BuildWindows();
            BuildAndroid();
        }

        [MenuItem("MyBuilds/0 - Build + Run All")]
        public static void BuildRunAll()
        {
            BuildRunWindows();
            BuildRunAndroid();
        }
        
        [MenuItem("MyBuilds/0 - Run All")]
        public static void RunAll()
        {
            RunWindows();
            RunAndroidS22();
        }
    
        [MenuItem("MyBuilds/1 - Windows Build")]
        public static void BuildWindows()
        {
            string path = GetPath("Windows");
            if (path.Length == 0) return;
            
            BuildPipeline.BuildPlayer(Levels, path + $"/{ArtifactName}.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
        }
    
        [MenuItem("MyBuilds/1 - Windows Build + Run")]
        public static void BuildRunWindows()
        {
            string path = GetPath("Windows");
            if (path.Length == 0) return;
            
            BuildPipeline.BuildPlayer(Levels, path + $"/{ArtifactName}.exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
        }
    
        [MenuItem("MyBuilds/1 - Windows Run")]
        public static void RunWindows()
        {
            string path = GetPath("Windows");
            if (path.Length == 0) return;

            Process proc = new Process();
            proc.StartInfo.FileName = path + $"/{ArtifactName}.exe";
            proc.Start();
        }

        [MenuItem("MyBuilds/2 - Android Build")]
        public static void BuildAndroid()
        {
            string path = GetPath("Android");
            if (path.Length == 0) return;
            
            BuildPipeline.BuildPlayer(Levels, path + $"/{ArtifactName}.apk", BuildTarget.Android, BuildOptions.None);
        }

        [MenuItem("MyBuilds/2 - Android Build + Run")]
        public static void BuildRunAndroid()
        {
            string path = GetPath("Android");
            if (path.Length == 0) return;

            BuildPipeline.BuildPlayer(Levels, path + $"/{ArtifactName}.apk", BuildTarget.Android, BuildOptions.AutoRunPlayer);
        }

        [MenuItem("MyBuilds/2 - Android Run (S22)")]
        public static void RunAndroidS22()
        {
            // TODO: Get the default device dynamically, no clue how because Unity doesn't tell you shit
            string deviceName = "R3CT30BMB3Z"; // Galaxy S22
            string packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            
            string[] adbCommand = {"-s", deviceName, "shell", "am", "start", "-n", packageName + "/com.unity3d.player.UnityPlayerActivity"};
            // string[] adbCommand = {"-s", deviceName, "shell", "am", "start", "-n", packageName + "/com.unity3d.player.UnityPlayerActivity", "-e", "unity", "-systemallocator"}; // Pass CMDline arguments this way
            
            ADB.GetInstance().Run(adbCommand, "Did not work :(");
        }

        [MenuItem("MyBuilds/2 - Android Run (Quest 3)")]
        public static void RunAndroidQuest3()
        {
            // TODO: Get the default device dynamically, no clue how because Unity doesn't tell you shit
            string deviceName = "2G0YC1ZF8H0G17"; // Quest 3
            string packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            
            string[] adbCommand = {"-s", deviceName, "shell", "am", "start", "-n", packageName + "/com.unity3d.player.UnityPlayerActivity"};
            // string[] adbCommand = {"-s", deviceName, "shell", "am", "start", "-n", packageName + "/com.unity3d.player.UnityPlayerActivity", "-e", "unity", "-systemallocator"}; // Pass CMDline arguments this way
            
            ADB.GetInstance().Run(adbCommand, "Did not work :(");
        }
    }
}