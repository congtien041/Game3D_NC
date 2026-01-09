#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Startup
{
    static Startup()    
    {
        EditorPrefs.SetInt("showCounts_sportcarcgb3", EditorPrefs.GetInt("showCounts_sportcarcgb3") + 1);

        if (EditorPrefs.GetInt("showCounts_sportcarcgb3") == 1)       
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/packs/complete-off-road-racing-lit-edition-vol-1-353072");
            // System.IO.File.Delete("Assets/SportCar/Racing_Game.cs");
        }
    }     
}
#endif
