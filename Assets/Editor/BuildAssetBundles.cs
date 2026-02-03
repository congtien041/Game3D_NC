using System;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundles
{
    [MenuItem("Assets/Build Asset Bundles")]
    public static void BuildAllAssetBundles()
    {
        string bundleDirectory = "Assets/StreamingAssets/Bundles";
        if (!System.IO.Directory.Exists(bundleDirectory))
        {
            System.IO.Directory.CreateDirectory(bundleDirectory);
        }

        try
        {
            // set bundle name and build
            BuildPipeline.BuildAssetBundles(bundleDirectory, BuildAssetBundleOptions.None, 
                EditorUserBuildSettings.activeBuildTarget);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        
    }
}
