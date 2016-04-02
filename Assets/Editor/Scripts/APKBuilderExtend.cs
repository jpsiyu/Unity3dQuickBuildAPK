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
    BuildOption buildIndex = BuildOption.ExportAndPack;

    enum BuildOption
    {
        ExportAndPack,
        Export,
        Pack,
    };

    BuildSDKPlatform sdkIndex = BuildSDKPlatform.YinHan;
    enum BuildSDKPlatform
    {
        YinHan,
        FeiYu,
        UC,
        QQ,
        Wechat,
        SDK_91,
    };

    ServerAddr addrIndex = ServerAddr.Http1;
    enum ServerAddr
    {
        Http1,
        Http2,
        Http3,
    };
        
    #endregion


    #region update inspector
    private float popupWidth = 150;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InspectorForSDKPlatform();
        InspectorForServerAddr();
        InspectorForBuildOption();
        InspectorForRun();
    }

    private void InspectorForRun() {
        if (GUILayout.Button("Run...(^.^)", GUILayout.Width(100)))
        {
            Run();
        }
    }

    /// <summary>
    /// For Server Addr
    /// </summary>
    private void InspectorForServerAddr() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("SAddr");
        GUILayout.FlexibleSpace();
        string[] popString = {ServerAddr.Http1.ToString(),
                             ServerAddr.Http2.ToString(),
                             ServerAddr.Http3.ToString()};
        addrIndex = (ServerAddr)EditorGUILayout.Popup((int)addrIndex, popString);
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// For SDK Platform
    /// </summary>
    private void InspectorForSDKPlatform() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("SDK");
        GUILayout.FlexibleSpace();
        string[] popString = {BuildSDKPlatform.YinHan.ToString(),
                             BuildSDKPlatform.FeiYu.ToString(),
                             BuildSDKPlatform.QQ.ToString(),
                             BuildSDKPlatform.Wechat.ToString(),
                             BuildSDKPlatform.UC.ToString(),
                             BuildSDKPlatform.SDK_91.ToString()};
        sdkIndex = (BuildSDKPlatform)EditorGUILayout.Popup((int)sdkIndex, popString);
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// For Build Option
    /// </summary>
    private void InspectorForBuildOption() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Build");
        GUILayout.FlexibleSpace();
        string[] popString = {BuildOption.ExportAndPack.ToString(), 
                              BuildOption.Export.ToString(), 
                              BuildOption.Pack.ToString()};
        buildIndex = (BuildOption)EditorGUILayout.Popup((int)buildIndex, popString);
        GUILayout.EndHorizontal();
    }
    #endregion

    private void Run() {
        string log = "Start Run:{0},{1},{2}";
        UnityEngine.Debug.Log(string.Format(
            log, 
            addrIndex.ToString(),
            sdkIndex.ToString(),
            buildIndex.ToString()));
        switch (buildIndex) { 
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
