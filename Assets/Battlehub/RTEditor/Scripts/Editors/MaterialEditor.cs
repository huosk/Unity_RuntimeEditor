﻿using UnityEngine;
using System.Collections;

using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Battlehub.Utils;
using System;
using System.Collections.Generic;

using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2;
using Battlehub.RTSaveLoad2.Interface;
#if PROC_MATERIAL
using ProcPropertyDescription = UnityEngine.ProceduralPropertyDescription;
using ProcPropertyType = UnityEngine.ProceduralPropertyType;
#endif
namespace Battlehub.RTEditor
{
 

    public class MaterialPropertyDescriptor
    {
        public object Target;
        public string Label;
        public RTShaderPropertyType Type;
#if !UNITY_WEBGL && PROC_MATERIAL
        public ProcPropertyDescription ProceduralDescription;
#endif
        public PropertyInfo PropertyInfo;
        public RuntimeShaderInfo.RangeLimits Limits;
        public TextureDimension TexDims;
        
        public PropertyEditorCallback ValueChangedCallback;

        public MaterialPropertyDescriptor(object target, string label, RTShaderPropertyType type, PropertyInfo propertyInfo, RuntimeShaderInfo.RangeLimits limits, TextureDimension dims, PropertyEditorCallback callback)
        {
            Target = target;
            Label = label;
            Type = type;
            PropertyInfo = propertyInfo;
            Limits = limits;
            TexDims = dims;
            ValueChangedCallback = callback;
        }
#if !UNITY_WEBGL && PROC_MATERIAL
        public MaterialPropertyDescriptor(object target, string label, ProcPropertyDescription procDescription, PropertyInfo propertyInfo, PropertyEditorCallback callback)
        {
            Target = target;
            Label = label;
            Type = RTShaderPropertyType.Procedural;
            ProceduralDescription = procDescription;
            PropertyInfo = propertyInfo;
            ValueChangedCallback = callback;
        }
#endif
    }


    public interface IMaterialDescriptor
    {
        string ShaderName
        {
            get;
        }

        object CreateConverter(MaterialEditor editor);

        MaterialPropertyDescriptor[] GetProperties(MaterialEditor editor, object converter);
    }

    public class MaterialEditor : MonoBehaviour
    {
        private static Dictionary<string, IMaterialDescriptor> m_propertySelectors;
        static MaterialEditor()
        {
            var type = typeof(IMaterialDescriptor);

            var types = Reflection.GetAssignableFromTypes(type);

            m_propertySelectors = new Dictionary<string, IMaterialDescriptor>();
            foreach (Type t in types)
            {
                IMaterialDescriptor selector = (IMaterialDescriptor)Activator.CreateInstance(t);
                if (selector == null)
                {
                    Debug.LogWarningFormat("Unable to instantiate selector of type " + t.FullName);
                    continue;
                }
                if (selector.ShaderName == null)
                {
                    Debug.LogWarningFormat("ComponentType is null. Selector ShaderName is null {0}", t.FullName);
                    continue;
                }
                if (m_propertySelectors.ContainsKey(selector.ShaderName))
                {
                    Debug.LogWarningFormat("Duplicate component selector for {0} found. Type name {1}. Using {2} instead", selector.ShaderName, selector.GetType().FullName, m_propertySelectors[selector.ShaderName].GetType().FullName);
                }
                else
                {
                    m_propertySelectors.Add(selector.ShaderName, selector);
                }
            }
        }
        
        [SerializeField]
        private RangeEditor RangeEditor = null;

        [SerializeField]
        private Image m_image = null;
        [SerializeField]
        private Text TxtMaterialName = null;
        [SerializeField]
        private Text TxtShaderName = null;

        [SerializeField]
        private Transform EditorsPanel = null;

        [HideInInspector]
        public Material Material = null;

        private IRuntimeEditor m_editor;
        private IResourcePreviewUtility m_resourcePreviewUtility;
        private Texture2D m_previewTexture;

        private void Start()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_resourcePreviewUtility = IOC.Resolve<IResourcePreviewUtility>();

            if (Material == null)
            {
                Material = m_editor.Selection.activeObject as Material;
            }

            if (Material == null)
            {
                Debug.LogError("Select material");
                return;
            }

            m_previewTexture = new Texture2D(1, 1, TextureFormat.ARGB32, true);

            TxtMaterialName.text = Material.name;
            if (Material.shader != null)
            {
                TxtShaderName.text = Material.shader.name;
            }
            else
            {
                TxtShaderName.text = "Shader missing";
            }

    
            UpdatePreview(Material);

            BuildEditor();
        }

        private void OnDestroy()
        {
            if(m_previewTexture != null)
            {
                Destroy(m_previewTexture);
            }
        }

        public void BuildEditor()
        {
            foreach(Transform t in EditorsPanel)
            {
                Destroy(t.gameObject);
            }

            IMaterialDescriptor selector;
            if(!m_propertySelectors.TryGetValue(Material.shader.name, out selector))
            {
                selector = new MaterialDescriptor();
            }


            object converter = selector.CreateConverter(this);
            MaterialPropertyDescriptor[] descriptors = selector.GetProperties(this, converter);
            if(descriptors == null)
            {
                Destroy(gameObject);
                return;
            }

            for(int i = 0; i < descriptors.Length; ++i)
            {
                MaterialPropertyDescriptor descriptor = descriptors[i];
                PropertyEditor editor = null;
                object target = descriptor.Target;
                PropertyInfo propertyInfo = descriptor.PropertyInfo;
#if !UNITY_WEBGL && PROC_MATERIAL
                if(descriptor.ProceduralDescription == null)
#endif
                {
                    RTShaderPropertyType propertyType = descriptor.Type;

                    switch (propertyType)
                    {
                        case RTShaderPropertyType.Range:
                            if(RangeEditor != null)
                            {
                                RangeEditor range = Instantiate(RangeEditor);
                                range.transform.SetParent(EditorsPanel, false);

                                var rangeLimits = descriptor.Limits;
                                range.Min = rangeLimits.Min;
                                range.Max = rangeLimits.Max;
                                editor = range;
                            }
                            break;
                        default:
                            if (EditorsMap.IsPropertyEditorEnabled(propertyInfo.PropertyType))
                            {
                                GameObject editorPrefab = EditorsMap.GetPropertyEditor(propertyInfo.PropertyType);
                                GameObject instance = Instantiate(editorPrefab);
                                instance.transform.SetParent(EditorsPanel, false);

                                if (instance != null)
                                {
                                    editor = instance.GetComponent<PropertyEditor>();
                                }
                            }
                            break;
                    }
                }
#if !UNITY_WEBGL && PROC_MATERIAL
                else
                {
                    ProcPropertyDescription input = descriptor.ProceduralDescription;
                    if(input.hasRange)
                    {
                        if(input.type == ProcPropertyType.Float)
                        {
                            if(RangeEditor != null)
                            {
                                RangeEditor range = Instantiate(RangeEditor);
                                range.transform.SetParent(EditorsPanel, false);
                                range.Min = input.minimum;
                                range.Max = input.maximum;
                                //TODO implement step on range editor // = input.step
                                editor = range;
                            }
                        }
                        else
                        {
                            //TODO: Implement range on vector editors

                            if (EditorsMap.IsPropertyEditorEnabled(propertyInfo.PropertyType))
                            {
                                GameObject editorPrefab = EditorsMap.GetPropertyEditor(propertyInfo.PropertyType);
                                GameObject instance = Instantiate(editorPrefab);
                                instance.transform.SetParent(EditorsPanel, false);

                                if (instance != null)
                                {
                                    editor = instance.GetComponent<PropertyEditor>();
                                }
                            }
                        }
                    }
                    else
                    {
                        //if(input.type == ProceduralPropertyType.Enum)
                        //TODO: Implement enum from string array editor. //input.enumOptions

                        if (EditorsMap.IsPropertyEditorEnabled(propertyInfo.PropertyType))
                        {
                            GameObject editorPrefab = EditorsMap.GetPropertyEditor(propertyInfo.PropertyType);
                            GameObject instance = Instantiate(editorPrefab);
                            instance.transform.SetParent(EditorsPanel, false);

                            if (instance != null)
                            {
                                editor = instance.GetComponent<PropertyEditor>();
                            }
                        }
                    }
                }
#endif

                if (editor == null)
                {
                    continue;
                }

                editor.Init(target, propertyInfo, descriptor.Label, null, descriptor.ValueChangedCallback, () => 
                {
                    m_editor.IsDirty = true;
                    UpdatePreview(Material);
                });
            }
        }


        private PropertyEditor InstantiateEditor( PropertyInfo propertyInfo)
        {
            PropertyEditor editor = null;
            if (EditorsMap.IsPropertyEditorEnabled(propertyInfo.PropertyType))
            {
                GameObject prefab = EditorsMap.GetPropertyEditor(propertyInfo.PropertyType);
                if (prefab != null)
                {
                    editor = Instantiate(prefab).GetComponent<PropertyEditor>();
                    editor.transform.SetParent(EditorsPanel, false);
                }
            }

            return editor;
        }

        private void UpdatePreview(Material material)
        {
            m_editor.UpdatePreview(material, assetItem =>
            {
                if (m_image != null && assetItem != null)
                {
                    m_previewTexture.LoadImage(assetItem.Preview.PreviewData);
                    m_image.sprite = Sprite.Create(m_previewTexture, new Rect(0, 0, m_previewTexture.width, m_previewTexture.height), new Vector2(0.5f, 0.5f));
                }
            });
        }

        //private int m_updateCounter = 0;
        //private void Update()
        //{
        //    m_updateCounter++;
        //    m_updateCounter %= 120;
        //    if (m_updateCounter == 0)
        //    {
        //        UpdatePreview(Material);
        //    }
        //}
    }
}

