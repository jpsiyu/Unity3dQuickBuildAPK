using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Diagnostics;
using System.Threading;


[CustomEditor(typeof(APKBuilder), true)]
public class APKBuilderExtend : Editor
{


    #region setting path
    private static string productName = PlayerSettings.productName;

    private static string AndroidProjectPath = 
        Application.dataPath + "/../AndroidProject/";

    private static string ProjectRootPath = 
        AndroidProjectPath + productName + "\\";

    private static string antPath =
        Application.dataPath + "/../Ant/";
    #endregion

    #region project args
    BuildOption popupIndex = BuildOption.ExportAndPack;

    enum BuildOption
    {
        ExportAndPack,
        Export,
        Pack,
    };
        
    #endregion


    #region update inspector
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Build");
        string[] popString = {BuildOption.ExportAndPack.ToString(), 
                              BuildOption.Export.ToString(), 
                              BuildOption.Pack.ToString()};
        popupIndex = (BuildOption)EditorGUILayout.Popup((int)popupIndex, popString);
        if (GUILayout.Button("Run")) {
            Run();
        }
        GUILayout.EndHorizontal();

        //GUILayout.Space(30);

        //if (GUILayout.Button("Export Project")) {
        //    UnityEngine.Debug.Log("Start Exporting...");
        //    EditorApplication.delayCall += ExportProject;
        //    UnityEngine.Debug.Log("Finish Exporting!");
        //}

        //GUILayout.Space(15);

        //if (GUILayout.Button("BuildAPK")) {
        //    UnityEngine.Debug.Log("Start Building...");
        //    EditorApplication.delayCall += RunCMDThread;
        //    //UnityEngine.Debug.Log("Finish Building!");
        //}

        //GUILayout.Space(15);

        //if (GUILayout.Button("Export And BuildAPK"))
        //{
        //    UnityEngine.Debug.Log("Start Export And BuildAPK...");
        //    EditorApplication.delayCall += ExportProject;
        //    EditorApplication.delayCall += RunCMDThread;
        //    //UnityEngine.Debug.Log("Finish Building!");
        //}
        //GUILayout.Space(30);
    }
    #endregion

    private void Run() {
        UnityEngine.Debug.Log(string.Format("Start Run {0}", popupIndex.ToString()));
        switch (popupIndex) { 
            case BuildOption.ExportAndPack:
                EditorApplication.delayCall += ExportProject;
                EditorApplication.delayCall += RunCMDThread;
                break;
            case BuildOption.Export:
                EditorApplication.delayCall += ExportProject;
                break;
            case BuildOption.Pack:
                EditorApplication.delayCall += RunCMDThread;
                break;
            default:
                break;
        }
        UnityEngine.Debug.Log("Run Finished");
    }

    #region export as android project
    public static void ExportProject() {
        string[] levels = new string[] { 
            "Assets/Scenes/Main.unity", 
            "Assets/Scenes/Copy.unity"
        };

        BuildPipeline.BuildPlayer(
            levels,
            Application.dataPath + "/../AndroidProject/",
            BuildTarget.Android,
            BuildOptions.AcceptExternalModificationsToPlayer);
    }
    #endregion


    #region run cmd and use ant release to do package
    public static void RunCMDThread() {
        Thread thread = new Thread(new ThreadStart(RunCDMNoReturn));
        thread.Start();
    }

    /*  1. enter exported project root
        2. copy ant file
        3. ant release -> apk       */
    private static string GetCMD() {

        string cmdEnterFolder = "cd " + ProjectRootPath.Replace("/", "\\");

        string cmdAntCopy = "&& copy /n " + antPath.Replace("/", "\\") + "*" + " .";

        string cmdAntRelease = "&& ant release";

        return cmdEnterFolder + cmdAntCopy + cmdAntRelease;
    }

    private static void RunCDMNoReturn() {
        string log = RunCMD();
        UnityEngine.Debug.Log(log);
        UnityEngine.Debug.Log("Finish Building!");
    }

    /* use a thread to do this (don't block main thread )*/
    private static string RunCMD() {
        string cmd = GetCMD();
        Process p = new Process();

        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.Arguments = "/c" + cmd;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = false;

        p.Start();
        return p.StandardOutput.ReadToEnd();
    }
    #endregion
}
