using UnityEngine;

public class AssetBundleLoader : MonoBehaviour {
    public const string AssetResourceRootPath = "Assets/Resources/Prefabs/";
    public const string BundleOutputRootPath = "Assets/Bundles/";

    class Asset {
        public string assetBundleName;
    }

    // Use this for initialization
    void Start () {
        string currentTestFolder = "TestLoadDependedAssetBundle/";
        string mainManifestName = currentTestFolder.Trim('/');

        //AssetImporter assetA = AssetImporter.GetAtPath(AssetResourceRootPath + currentTestFolder + "A.prefab");
        Asset assetA = new Asset { assetBundleName = "a_testloaddependedassetbundle" };
        UICamera uiCamera = GameObject.FindObjectOfType<UICamera>();



        AssetBundle mainManifestAssetBundle = AssetBundle.LoadFromFile(BundleOutputRootPath + currentTestFolder + mainManifestName);
        // Note: must assign "AssetBundleManifest"
        AssetBundleManifest mainManifest = mainManifestAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        mainManifestAssetBundle.Unload(false);

        AssetBundle assetABundle = AssetBundle.LoadFromFile(BundleOutputRootPath + currentTestFolder + assetA.assetBundleName);
        foreach (string name in assetABundle.GetAllAssetNames()) {
            GameObject gameObj = assetABundle.LoadAsset(name) as GameObject;
            NGUITools.AddChild(uiCamera.gameObject, gameObj);
            Debug.Log("assetABundle.LoadAsset " + assetABundle.LoadAsset(name));
        }
        assetABundle.Unload(false);

        // just load depended asset bundle, don't need assign, and don't care order
        int loaded = 0;
        foreach (string name in mainManifest.GetAllDependencies(assetA.assetBundleName)) {
            AssetBundle dependedAssetBundle = AssetBundle.LoadFromFile(BundleOutputRootPath + currentTestFolder + name);
            // Note: if want to do unload bundle, must do LoadAllAssets
            dependedAssetBundle.LoadAllAssets();
            dependedAssetBundle.Unload(false);
            // can't do unload, or mapping will lose
            if (loaded++ > 0)
                break;
        }
    }
}
