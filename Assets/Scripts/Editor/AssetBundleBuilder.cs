using UnityEngine;
using UnityEditor;
using System.IO;

public static class AssetBundleBuilder
{
    public const string AssetResourceRootPath = "Assets/Resources/Prefabs/";
    public const string BundleOutputRootPath = "Assets/Bundles/";
    [MenuItem("AssetBundleBuilder/TestCollectDependence")]
    static void TestCollectDependence()
    {
        string currentTestFolder = "TestCollectDependence/";
        AssetImporter assetA = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "A.prefab");

        AssetBundleBuild assetABundleBuild = new AssetBundleBuild();
        assetABundleBuild.assetBundleName = "A.ab";
        assetABundleBuild.assetNames = new string[] { assetA.assetPath };

        BuildPipeline.BuildAssetBundles(BundleOutputRootPath + currentTestFolder,
            new AssetBundleBuild[] { assetABundleBuild },
            BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle,
            BuildTarget.Android);

        FileInfo assetABundleFile = new FileInfo(BundleOutputRootPath + currentTestFolder + assetABundleBuild.assetBundleName);

        // if > 1MB, mean asset bundle will collect all dependence data
        Debug.LogFormat("assetABundleFile size = {0} bytes, is > 1MB = {1}", assetABundleFile.Length, assetABundleFile.Length > 1000000);

        // result: now building asset bundle always collect dependencies, one way solution is 
        //         BuildPipeline.PushAssetDependencies and BuildPipeline.PopAssetDependencies.
        //         the other way is do move out relative resource to outside project.
    }

    // Note: You should call this function after modify B.prefab to check last modified time.
    [MenuItem("AssetBundleBuilder/TestIncrementBuild")]
    static void TestIncrementBuild()
    {
        string currentTestFolder = "TestIncrementBuild/";
        AssetImporter assetA = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "A.prefab");
        AssetImporter assetB = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "B.prefab");
        

        AssetBundleBuild assetABundleBuild = new AssetBundleBuild();
        assetABundleBuild.assetBundleName = "A.ab";
        assetABundleBuild.assetNames = new string[] { assetA.assetPath };

        AssetBundleBuild assetBBundleBuild = new AssetBundleBuild();
        assetBBundleBuild.assetBundleName = "B.ab";
        assetBBundleBuild.assetNames = new string[] { assetB.assetPath };

        BuildPipeline.BuildAssetBundles(BundleOutputRootPath + currentTestFolder,
            new AssetBundleBuild[] { assetABundleBuild, assetBBundleBuild },
            BuildAssetBundleOptions.None,
            BuildTarget.Android);

        FileInfo assetABundleFile = new FileInfo(BundleOutputRootPath + currentTestFolder + assetABundleBuild.assetBundleName);
        FileInfo assetBBundleFile = new FileInfo(BundleOutputRootPath + currentTestFolder + assetBBundleBuild.assetBundleName);
        Debug.LogFormat("{0} LastWriteTime = {1}", assetABundleBuild.assetBundleName, assetABundleFile.LastWriteTime);
        Debug.LogFormat("{0} LastWriteTime = {1}", assetBBundleBuild.assetBundleName, assetBBundleFile.LastWriteTime);

        // result: there will generate each manifest for each bundle, and each manifest will have specific hash code for checking
        //         increment build or not.
    }

    [MenuItem("AssetBundleBuilder/TestExportBundleToSubFolder")]
    static void TestExportBundleToSubFolder()
    {
        string currentTestFolder = "TestExportBundleToSubFolder/";
        AssetImporter assetA = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "A.prefab");

        AssetBundleBuild assetABundleBuild = new AssetBundleBuild();
        assetABundleBuild.assetBundleName = "SubFolder/A.ab";
        assetABundleBuild.assetNames = new string[] { assetA.assetPath };
        
        BuildPipeline.BuildAssetBundles(BundleOutputRootPath + currentTestFolder,
            new AssetBundleBuild[] { assetABundleBuild },
            BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle,
            BuildTarget.Android);

        FileInfo manifestFile = new FileInfo(BundleOutputRootPath + currentTestFolder + "SubFolder.manifest");
        Debug.LogFormat("manifestFile = {0}, is exist = {1}", manifestFile.FullName, manifestFile.Exists);

        // result: total dependency list always in output folder. so if we have bundle in different folders, we need combine each folder dependency list.
    }

    // Note: should set A.prefab and B.prefab to same name in inspector
    [MenuItem("AssetBundleBuilder/TestBuildIntoSameBundleBySameName")]
    static void TestBuildIntoSameBundleBySameName()
    {
        string currentTestFolder = "TestBuildIntoSameBundleBySameName/";

        // Note: this will build all assets which have assigned asset bundle name
        BuildPipeline.BuildAssetBundles(
            BundleOutputRootPath + currentTestFolder, 
            BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle, 
            BuildTarget.Android);

        
        AssetImporter assetA = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "A.prefab");
        AssetImporter assetB = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "B.prefab");

        AssetBundleBuild testIntoSameBundleManual = new AssetBundleBuild();
        testIntoSameBundleManual.assetBundleName = assetA.assetBundleName + "-manual";
        testIntoSameBundleManual.assetNames = new string[] { assetA.assetPath, assetB.assetPath };

        BuildPipeline.BuildAssetBundles(
            BundleOutputRootPath + currentTestFolder,
            new AssetBundleBuild[] { testIntoSameBundleManual },
            BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle,
            BuildTarget.Android);

        FileInfo testIntoSameBundleFile = new FileInfo(BundleOutputRootPath + currentTestFolder + assetA.assetBundleName);
        FileInfo testIntoSameBundleManualFile = new FileInfo(BundleOutputRootPath + currentTestFolder + testIntoSameBundleManual.assetBundleName);

        // Note: we can find out that asset bundle size only half of A.prefab and B.prefab individual bundle size summation.
        //       which mean if prefabs reference the same assets will only have one copy.
        // Note: this only limit in the same bundle
        Debug.LogFormat("{0} size = {1} bytes, {2} size = {3} bytes, size is the same = {4}",
            assetA.assetBundleName, testIntoSameBundleFile.Length,
            testIntoSameBundleManual.assetBundleName, testIntoSameBundleManualFile.Length,
            testIntoSameBundleFile.Length == testIntoSameBundleManualFile.Length);

        // result: if have assigned same bundle name to different assets, the build all will build these asset into the same bundle.
        //         this action is the same as we set different assets into the same AssetBundleBuild.
    }

    [MenuItem("AssetBundleBuilder/TestBuildAllAndSetBundleNameToEachAsset")]
    static void TestBuildAllAndSetBundleNameToEachAsset()
    {
        string currentTestFolder = "TestBuildAllAndSetBundleNameToEachAsset/";
        // Note: this will build all assets which have assigned asset bundle name
        BuildPipeline.BuildAssetBundles(
            BundleOutputRootPath + currentTestFolder,
            BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle,
            BuildTarget.Android);
        // result: if depended asset have asset bundle name, it will be build alone to a single asset bundle,
        //         and other asset bundle will have this asset in dependencies array. 
    }
    // note: we can't set bundle version number to asset bundle variant, because we don't want have duplicate asset just set
    //       variant different


    [MenuItem("AssetBundleBuilder/TestCustomBuildAndSetBundleNameToEachAsset")]
    static void TestCustomBuildAndSetBundleNameToEachAsset()
    {
        string currentTestFolder = "TestCustomBuildAndSetBundleNameToEachAsset/";
        
        AssetImporter assetA = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "A.prefab");
        AssetImporter assetB = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "B.prefab");


        AssetBundleBuild assetABundleBuild = new AssetBundleBuild();
        assetABundleBuild.assetBundleName = "A.ab";
        assetABundleBuild.assetNames = new string[] { assetA.assetPath };

        AssetBundleBuild assetBBundleBuild = new AssetBundleBuild();
        assetBBundleBuild.assetBundleName = "B.ab";
        assetBBundleBuild.assetNames = new string[] { assetB.assetPath };

        BuildPipeline.BuildAssetBundles(BundleOutputRootPath + currentTestFolder,
            new AssetBundleBuild[] { assetABundleBuild, assetBBundleBuild },
            BuildAssetBundleOptions.None,
            BuildTarget.Android);

        // The array of AssetBundleBuild elements that is passed to the function is known as the "building map" 
        // result: failed, because BuildAssetBundles for AssetBundleBuild API description is 
        //         "The array of AssetBundleBuild elements that is passed to the function is known as the "building map" ",
        //         so it won't create asset dependencies. 
    }

    [MenuItem("AssetBundleBuilder/TestLoadDependedAssetBundle")]
    static void TestLoadDependedAssetBundle()
    {
        string currentTestFolder = "TestLoadDependedAssetBundle/";
        string mainManifestName = currentTestFolder.Trim('/');

        // Note: this will build all assets which have assigned asset bundle name
        BuildPipeline.BuildAssetBundles(
            BundleOutputRootPath + currentTestFolder,
            BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle,
            BuildTarget.Android);


        AssetImporter assetA = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "A.prefab");
        UICamera uiCamera = GameObject.FindObjectOfType<UICamera>();


        AssetBundle mainManifestAssetBundle = AssetBundle.LoadFromFile(BundleOutputRootPath + currentTestFolder + mainManifestName);
        // Note: must assign "AssetBundleManifest"
        AssetBundleManifest mainManifest = mainManifestAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        mainManifestAssetBundle.Unload(false);

        AssetBundle assetABundle = AssetBundle.LoadFromFile(BundleOutputRootPath + currentTestFolder + assetA.assetBundleName);
        foreach (string name in assetABundle.GetAllAssetNames())
        {
            GameObject gameObj = assetABundle.LoadAsset(name) as GameObject;
            NGUITools.AddChild(uiCamera.gameObject, gameObj);
            Debug.Log("assetABundle.LoadAsset " + assetABundle.LoadAsset(name));
        }
        assetABundle.Unload(false);

        // just load depended asset bundle, don't need assign, and don't care order
        int loaded = 0;
        foreach (string name in mainManifest.GetAllDependencies(assetA.assetBundleName))
        {
            AssetBundle dependedAssetBundle = AssetBundle.LoadFromFile(BundleOutputRootPath + currentTestFolder + name);
            // Note: if want to do unload bundle, must do LoadAllAssets
            dependedAssetBundle.LoadAllAssets();
            dependedAssetBundle.Unload(false);
            // can't do unload, or mapping will lose
            if (loaded++ > 0)
                break;
        }
    }

    [MenuItem("AssetBundleBuilder/TestCustomPushDependencies")]
    static void TestCustomPushDependencies()
    {
        string currentTestFolder = "TestCustomPushDependencies/";

        AssetImporter assetA = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "A.prefab");
        //AssetImporter assetB = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "B.prefab");

        RecursiveBuildAssetBundleDependencies(
            assetA.assetPath,
            BundleOutputRootPath + currentTestFolder);
        RecursivePopAsset(assetA.assetPath);

        // result: BuildPipeline.PushAssetDependencies() and BuildPipeline.PopAssetDependencies() no more work.
    }

    private static void RecursiveBuildAssetBundleDependencies(string assetPath, string bundleRootPath)
    {
        string[] dependedAssetPahes = AssetDatabase.GetDependencies(assetPath, false);
        foreach (string dependedAssetPah in dependedAssetPahes)
        {
            if (assetPath.Equals(dependedAssetPah)) continue;

            RecursiveBuildAssetBundleDependencies(dependedAssetPah, bundleRootPath);
        }
        BuildAssetBundle(assetPath, bundleRootPath);
    }

    private static void RecursivePopAsset(string assetPath)
    {
        string[] dependedAssetPahes = AssetDatabase.GetDependencies(assetPath, false);
        foreach (string dependedAssetPah in dependedAssetPahes)
        {
            if (assetPath.Equals(dependedAssetPah)) continue;

            RecursivePopAsset(dependedAssetPah);
        }
        BuildPipeline.PopAssetDependencies();
    }

    private static void BuildAssetBundle(string assetPath, string bundleRootPath)
    {
        BuildPipeline.PushAssetDependencies();

        AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
        assetBundleBuild.assetBundleName = Path.GetFileNameWithoutExtension(assetPath);
        assetBundleBuild.assetNames = new string[] { assetPath };

        BuildPipeline.BuildAssetBundles(
            bundleRootPath,
            new AssetBundleBuild[] { assetBundleBuild },
            BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildTarget.Android);
    }
}
