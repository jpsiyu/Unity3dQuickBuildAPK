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
    private const string projectDirName = "AndroidProject";

    private static string AndroidProjectPath = string.Format("{0}/../{1}/",
        Application.dataPath,
        projectDirName);

    private static string ProjectRootPath = 
        AndroidProjectPath + productName + "\\";

    private static string antPath =
        Application.dataPath + "/../Ant/";
    #endregion

    #region project args
    private static string sdk_path;
    private static string app_name;

    BuildOption buildIndex = BuildOption.ExportAndPack;

    enum BuildOption
    {
        ExportAndPack,
        Export,
        ModiManif,
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

    private string appName = "MyApp";
    private string appVersion = "0.73.2";


    private struct Advance {
        public bool showAdvance;
        public bool copySDK;
        public bool copyRes;
        public bool stopMultiTouch;
        public bool enableDebug;

        public void SetDefault() {
            showAdvance = false;
            copySDK = true;
            copyRes = true;
            stopMultiTouch = false;
            enableDebug = false;
        }
    };

    private Advance advanceOption;

    private int counter = 0;

    #endregion


    #region update inspector
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InspectorForProductName();
        InspectorForSDKPlatform();
        InspectorForServerAddr();
        InspectorForBuildOption();
        InspectorForAdvance();
        InspectorForCounter();
        InspectorForRun();

    }


    /// <summary>
    /// For Counter
    /// </summary>
    private void InspectorForCounter() {
        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Counter: {0}", counter.ToString()));
        if (GUILayout.Button("Reset")) {
            ResetCounter();
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// For Advance
    /// </summary>
    private void InspectorForAdvance() {
        advanceOption.showAdvance = GUILayout.Toggle(advanceOption.showAdvance, "Advance");
        if (advanceOption.showAdvance)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            advanceOption.copySDK = GUILayout.Toggle(advanceOption.copySDK, "CopySDK");
            advanceOption.copyRes = GUILayout.Toggle(advanceOption.copyRes, "CopyRes");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            advanceOption.enableDebug = GUILayout.Toggle(advanceOption.enableDebug, "EnableDebug");
            advanceOption.stopMultiTouch = GUILayout.Toggle(advanceOption.stopMultiTouch, "StopMultiTouch");
            GUILayout.EndHorizontal();
        }
        else {
            advanceOption.SetDefault();
        }

    }

    /// <summary>
    /// For User Input
    /// </summary>
    private void InspectorForProductName() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("AppName");
        GUILayout.FlexibleSpace();
        appName = GUILayout.TextField(appName, 10, GUILayout.Width(150));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Version");
        GUILayout.FlexibleSpace();
        appVersion = GUILayout.TextField(appVersion, 10, GUILayout.Width(150));
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// For Run Button
    /// </summary>
    private void InspectorForRun() {
        if (GUILayout.Button("Run...(^.^)", GUILayout.Width(100)))
        {
            Run();
            IncreCounter();
        }
    }

    private void IncreCounter() {
        counter++;
    }

    private void ResetCounter() {
        counter = 0;
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
                              BuildOption.ModiManif.ToString(),
                              BuildOption.Pack.ToString()};
        buildIndex = (BuildOption)EditorGUILayout.Popup((int)buildIndex, popString);
        GUILayout.EndHorizontal();
    }
    #endregion

    private void Run() {

        // get last info
        sdk_path = EditorPrefs.GetString("AndroidSdkRoot");
        app_name = PlayerSettings.productName;

        string log = "Start Run:{0},{1},{2}";
        UnityEngine.Debug.Log(string.Format(
            log, 
            addrIndex.ToString(),
            sdkIndex.ToString(),
            buildIndex.ToString()));
        switch (buildIndex) { 
            case BuildOption.ExportAndPack:
                EditorApplication.delayCall += ExportProject;
                EditorApplication.delayCall += PackThread;
                break;
            case BuildOption.Export:
                EditorApplication.delayCall += ExportProject;
                break;
            case BuildOption.ModiManif:
                EditorApplication.delayCall += ModiThread;
                break;
            case BuildOption.Pack:
                EditorApplication.delayCall += PackThread;
                break;
            default:
                break;
        }
        UnityEngine.Debug.Log("Run Finished");
    }

    #region export as android project
    public static void ExportProject() {

        int len = EditorBuildSettings.scenes.Length;
        string[] levels = new string[len];
        for(int i=0; i<len; i++){
            levels[i] = EditorBuildSettings.scenes[i].path;
        }


        BuildPipeline.BuildPlayer(
            levels,
            AndroidProjectPath,
            BuildTarget.Android,
            BuildOptions.AcceptExternalModificationsToPlayer);
    }
    #endregion


    #region run cmd and use ant release to do package
    private static void PackThread() {

        Thread thread = new Thread(new ThreadStart(Pack));
        thread.Start();
    }

    private static void ModiThread()
    {
        Thread thread = new Thread(new ThreadStart(Modi));
        thread.Start();
    }

    /*  1. enter exported project root
        2. copy ant file
        3. ant release -> apk       */
    private static string GetPackCMD() {

        string cmdCD = string.Format("cd {0} && copy /n {1}* . ",
            ProjectRootPath.Replace("/", "\\"), 
            antPath.Replace("/", "\\"));

        string cmdEchoFile = string.Format("ant echo_file -Dsdk_path={0} -Dapp_name={1}",
            sdk_path, app_name);

        string cmdRelease = "ant release";

        return string.Format("{0} && {1} && {2}",
            cmdCD, cmdEchoFile, cmdRelease);
    }

    private static string GetModiCMD() {
        return string.Format("cd {0} && ant modi", ProjectRootPath);
    }


    /* use a thread to do this (don't block main thread )*/
    private static void Pack() {
        string cmd = GetPackCMD();
        NewProcess(cmd);
    }

    private static void Modi() {
        string cmd = GetModiCMD();
        NewProcess(cmd);
    }

    private static void NewProcess(string cmd) {
        Process p = new Process();

        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.Arguments = "/c" + cmd;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = false;

        p.Start();
        string log = p.StandardOutput.ReadToEnd();
    }
    #endregion
}
