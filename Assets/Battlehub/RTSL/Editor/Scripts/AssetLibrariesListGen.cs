﻿using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSL
{
    public class AssetLibrariesListGen
    {
        public static int GetIdentity()
        {
            string path = "Assets" + RTSLPath.UserRoot + "/" + RTSLPath.LibrariesFolder + "/Resources/Lists/AssetLibrariesList.asset";
            AssetLibrariesListAsset list = AssetDatabase.LoadAssetAtPath<AssetLibrariesListAsset>(path);
            if(list == null)
            {
                return 0;
            }

            if(list.Identity >= AssetLibraryInfo.STATICLIB_LAST)
            {
                Debug.LogError("Asset Lib identifiers exhausted");
                return 0;
            }

            return list.Identity;
        }


        public static AssetLibrariesListAsset UpdateList(int identity = 0)
        {
            if(identity == 0)
            {
                identity = GetIdentity();
            }
            
            string dir = RTSLPath.UserRoot;
            string dataPath = Application.dataPath;

            if (!Directory.Exists(dataPath + dir))
            {
                Directory.CreateDirectory(dataPath + dir);
            }

            if (!Directory.Exists(dataPath + dir + "/" + RTSLPath.LibrariesFolder))
            {
                AssetDatabase.CreateFolder("Assets" + dir, RTSLPath.LibrariesFolder);
            }

            dir = dir + "/" + RTSLPath.LibrariesFolder;
            if (!Directory.Exists(dataPath + dir + "/Resources"))
            {
                AssetDatabase.CreateFolder("Assets" + dir, "Resources");
            }

            dir = dir + "/Resources";

            if (!Directory.Exists(dataPath + dir + "/Lists"))
            {
                AssetDatabase.CreateFolder("Assets" + dir, "Lists");
            }
            dir = dir + "/Lists";

            string path = "Assets" + RTSLPath.UserRoot + "/" + RTSLPath.LibrariesFolder + "/Resources/Lists/AssetLibrariesList.asset";

            AssetLibrariesListAsset asset = Create();
            asset.Identity = identity;
            
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            return asset;
        }

        private static AssetLibrariesListAsset Create()
        {
            AssetLibrariesListAsset asset = ScriptableObject.CreateInstance<AssetLibrariesListAsset>();
            asset.List = new List<AssetLibraryListEntry>();

            string[] assetLibraries = AssetDatabase.FindAssets("t:AssetLibraryAsset");
            for(int i = 0; i < assetLibraries.Length; ++i)
            {
                string assetLib = assetLibraries[i];

                assetLib = AssetDatabase.GUIDToAssetPath(assetLib);

                
                

                if (assetLib.StartsWith("Assets" + RTSLPath.UserRoot + "/" + RTSLPath.LibrariesFolder + "/Resources/Scenes/"))
                {
                    continue;
                }

                if (assetLib.StartsWith("Assets" + RTSLPath.UserRoot + "/" + RTSLPath.LibrariesFolder + "/Resources/BuiltInAssets"))
                {
                    continue;
                }

                if (!assetLib.Contains("/Resources/"))
                {
                    Debug.LogWarning("Move " + assetLib + " to Resources folder");
                    continue;
                }

                AssetLibraryAsset assetLibAsset = AssetDatabase.LoadAssetAtPath<AssetLibraryAsset>(assetLib);

                int index = assetLib.IndexOf("/Resources/");
                assetLib = assetLib.Remove(0, index + 11);

                asset.List.Add(new AssetLibraryListEntry { Library = assetLib, Ordinal = assetLibAsset.Ordinal });
            }

            return asset;
        }
    }
}
