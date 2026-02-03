using System;
using UnityEditor;
using UnityEngine;
using System.IO; // Thêm thư viện này để xử lý file/folder an toàn hơn

public class BuildAssetBundles
{
    // Đổi tên menu một chút để dễ nhận biết
    [MenuItem("Assets/Build Asset Bundles (LZ4 - Fast Load)")]
    public static void BuildAllAssetBundles()
    {
        string bundleDirectory = "Assets/StreamingAssets/Bundles";
        
        // Kiểm tra và tạo thư mục nếu chưa có
        if (!Directory.Exists(bundleDirectory))
        {
            Directory.CreateDirectory(bundleDirectory);
        }

        try
        {
            // --- THAY ĐỔI QUAN TRỌNG TẠI ĐÂY ---
            // Đổi 'None' thành 'ChunkBasedCompression'
            BuildPipeline.BuildAssetBundles(
                bundleDirectory, 
                BuildAssetBundleOptions.ChunkBasedCompression, // <--- CÁI NÀY GIÚP LOAD SIÊU NHANH
                EditorUserBuildSettings.activeBuildTarget
            );
            
            Debug.Log("<color=green>Build AssetBundle thành công! (Mode: LZ4)</color>");
            
            // Tự động mở thư mục chứa bundle lên cho tiện kiểm tra
            EditorUtility.RevealInFinder(bundleDirectory);
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi khi Build Bundle: " + e.Message);
        }
    }
}