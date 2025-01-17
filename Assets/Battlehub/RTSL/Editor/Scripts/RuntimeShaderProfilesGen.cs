﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTSL
{
    public class RuntimeShaderProfilesGen
    {
        private static readonly Dictionary<ShaderUtil.ShaderPropertyType, RTShaderPropertyType> m_typeToType = new Dictionary<ShaderUtil.ShaderPropertyType, RTShaderPropertyType>
            { { ShaderUtil.ShaderPropertyType.Color, RTShaderPropertyType.Color },
              { ShaderUtil.ShaderPropertyType.Float, RTShaderPropertyType.Float },
              { ShaderUtil.ShaderPropertyType.Range, RTShaderPropertyType.Range },
              { ShaderUtil.ShaderPropertyType.TexEnv, RTShaderPropertyType.TexEnv },
              { ShaderUtil.ShaderPropertyType.Vector, RTShaderPropertyType.Vector }};

        
        public static void CreateProfile()
        {
            RuntimeShaderProfilesAsset asset = Create();
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

            string path = "Assets" + RTSLPath.UserRoot + "/" + RTSLPath.LibrariesFolder + "/Resources/Lists/ShaderProfiles.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }


        private static RuntimeShaderProfilesAsset Create()
        {
            RuntimeShaderProfilesAsset asset = ScriptableObject.CreateInstance<RuntimeShaderProfilesAsset>();
            asset.ShaderInfo = new List<RuntimeShaderInfo>();

            ShaderInfo[] allShaderInfo = ShaderUtil.GetAllShaderInfo().OrderBy(si => si.name).ToArray();
            HashSet<string> shaderNames = new HashSet<string>();
            for(int i = 0; i < allShaderInfo.Length; ++i)
            {
                ShaderInfo shaderInfo = allShaderInfo[i];
                Shader shader = Shader.Find(shaderInfo.name);

                RuntimeShaderInfo profile = Create(shader);
                asset.ShaderInfo.Add(profile);

                if (shaderNames.Contains(shaderInfo.name))
                {
                    Debug.LogWarning("Shader with same name already exists. Consider renaming " + shaderInfo.name + " shader. File: " + AssetDatabase.GetAssetPath(shader));
                }
                else
                {
                    shaderNames.Add(shaderInfo.name);
                }
            }
            return asset;
        }

        private static RuntimeShaderInfo Create(Shader shader)
        {
            if (shader == null)
            {
                throw new System.ArgumentNullException("shader");
            }

            int propertyCount = ShaderUtil.GetPropertyCount(shader);

            RuntimeShaderInfo shaderInfo = new RuntimeShaderInfo();
            shaderInfo.Name = shader.name;
            shaderInfo.PropertyCount = propertyCount;
            shaderInfo.PropertyDescriptions = new string[propertyCount];
            shaderInfo.PropertyNames = new string[propertyCount];
            shaderInfo.PropertyRangeLimits = new RuntimeShaderInfo.RangeLimits[propertyCount];
            shaderInfo.PropertyTexDims = new TextureDimension[propertyCount];
            shaderInfo.PropertyTypes = new RTShaderPropertyType[propertyCount];
            shaderInfo.IsHidden = new bool[propertyCount];

            for (int i = 0; i < propertyCount; ++i)
            {
                shaderInfo.PropertyDescriptions[i] = ShaderUtil.GetPropertyDescription(shader, i);
                shaderInfo.PropertyNames[i] = ShaderUtil.GetPropertyName(shader, i);
                shaderInfo.PropertyRangeLimits[i] = new RuntimeShaderInfo.RangeLimits(
                    ShaderUtil.GetRangeLimits(shader, i, 0),
                    ShaderUtil.GetRangeLimits(shader, i, 1),
                    ShaderUtil.GetRangeLimits(shader, i, 2));
                shaderInfo.PropertyTexDims[i] = ShaderUtil.GetTexDim(shader, i);

                RTShaderPropertyType rtType = RTShaderPropertyType.Unknown;
                ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);
                if (m_typeToType.ContainsKey(type))
                {
                    rtType = m_typeToType[type];
                }

                shaderInfo.PropertyTypes[i] = rtType;
                shaderInfo.IsHidden[i] = ShaderUtil.IsShaderPropertyHidden(shader, i);
            }
            return shaderInfo;
        }
    }
}
