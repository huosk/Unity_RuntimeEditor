﻿using System;
using UnityEngine;

using Battlehub.UIControls;
using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using Battlehub.RTSL.Interface;

using UnityObject = UnityEngine.Object;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Battlehub.RTEditor
{
    public interface ISelectObjectDialog
    {
        Type ObjectType
        {
            get;
            set;
        }

        bool IsNoneSelected
        {
            get;
        }

        UnityObject SelectedObject
        {
            get;
        }
    }

    public class SelectObjectDialog : RuntimeWindow, ISelectObjectDialog
    {
        [SerializeField]
        private TMP_InputField m_filter = null;
        [SerializeField]
        private VirtualizingTreeView m_treeView = null;
        [SerializeField]
        private Toggle m_toggleAssets = null;

        public UnityObject SelectedObject
        {
            get;
            private set;
        }

        public Type ObjectType
        {
            get;
            set;
        }

        public bool IsNoneSelected
        {
            get;
            private set;
        }

        private Dialog m_parentDialog;
        private IWindowManager m_windowManager;
        private IProject m_project;
        private Guid m_noneGuid = Guid.NewGuid();
        private bool m_previewsCreated;
        private AssetItem[] m_assetsCache;
        private AssetItem[] m_sceneCache;
        private Dictionary<long, UnityObject> m_sceneObjects;

        protected override void AwakeOverride()
        {
            IOC.RegisterFallback<ISelectObjectDialog>(this);
            WindowType = RuntimeWindowType.SelectObject;
            base.AwakeOverride();
        }

        private void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.IsCancelVisible = true;
            m_parentDialog.OkText = "Select";
            m_parentDialog.CancelText = "Cancel";
            m_parentDialog.Ok += OnOk;

            m_toggleAssets.onValueChanged.AddListener(OnAssetsTabSelectionChanged);

            m_project = IOC.Resolve<IProject>();
            m_windowManager = IOC.Resolve<IWindowManager>();

            IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();
            AssetItem[] assetItems = m_project.Root.Flatten(true, false).Where(item =>
            {
                Type type = m_project.ToType((AssetItem)item);
                return type == ObjectType || type.IsSubclassOf(ObjectType);

            }).OfType<AssetItem>().ToArray();

            m_treeView.SelectionChanged += OnSelectionChanged;
            m_treeView.ItemDataBinding += OnItemDataBinding;
            Editor.IsBusy = true;
            m_parentDialog.IsOkInteractable = false;
            m_project.GetAssetItems(assetItems, (error, assetItemsWithPreviews) =>
            {
                if (error.HasError)
                {
                    Editor.IsBusy = false;
                    m_windowManager.MessageBox("Can't GetAssets", error.ToString());
                    return;
                }

                AssetItem none = new AssetItem();
                none.Name = "None";
                none.TypeGuid = m_noneGuid;

                assetItemsWithPreviews = new[] { none }.Union(assetItemsWithPreviews).ToArray();

                m_previewsCreated = false;
                StartCoroutine(ProjectItemView.CoCreatePreviews(assetItemsWithPreviews, m_project, resourcePreview, () =>
                {
                    m_previewsCreated = true;
                    HandleSelectionChanged((AssetItem)m_treeView.SelectedItem);
                    m_treeView.ItemDoubleClick += OnItemDoubleClick;
                    m_parentDialog.IsOkInteractable = m_previewsCreated && m_treeView.SelectedItem != null;
                    Editor.IsBusy = false;

                    if(m_filter != null)
                    {
                        if (!string.IsNullOrEmpty(m_filter.text))
                        {
                            ApplyFilter(m_filter.text);
                        }
                        m_filter.onValueChanged.AddListener(OnFilterValueChanged);
                    }
                }));

                m_assetsCache = assetItemsWithPreviews;
                m_treeView.Items = m_assetsCache;

                List<AssetItem> sceneCache = new List<AssetItem>();
                sceneCache.Add(none);

                m_sceneObjects = new Dictionary<long, UnityObject>();
                ExposeToEditor[] sceneObjects = Editor.Object.Get(false, true).ToArray();
                for(int i = 0; i < sceneObjects.Length; ++i)
                {
                    ExposeToEditor exposeToEditor = sceneObjects[i];
                    UnityObject obj = null;
                    if(ObjectType == typeof(GameObject))
                    {
                        obj = exposeToEditor.gameObject;
                    }
                    else if(ObjectType.IsSubclassOf(typeof(Component)))
                    {
                        obj = exposeToEditor.GetComponent(ObjectType);
                    }
                    
                    if(obj != null)
                    {
                        AssetItem assetItem = new AssetItem()
                        {
                            ItemID = m_project.ToID(exposeToEditor),
                            Name = exposeToEditor.name,
                            
                        };
                        assetItem.Preview = new Preview { ItemID = assetItem.ItemID, PreviewData = new byte[0] };
                        sceneCache.Add(assetItem);
                        m_sceneObjects.Add(assetItem.ItemID, obj);
                    }
                }
                m_sceneCache = sceneCache.ToArray();
            });
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
        
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
            }

            if(m_toggleAssets != null)
            {
                m_toggleAssets.onValueChanged.RemoveListener(OnAssetsTabSelectionChanged);
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDoubleClick -= OnItemDoubleClick;
                m_treeView.SelectionChanged -= OnSelectionChanged;
                m_treeView.ItemDataBinding -= OnItemDataBinding;
            }

            if (m_filter != null)
            {
                m_filter.onValueChanged.RemoveListener(OnFilterValueChanged);
            }

            IOC.UnregisterFallback<ISelectObjectDialog>(this);
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ProjectItem projectItem = e.Item as ProjectItem;
            if (projectItem == null)
            {
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = null;
                ProjectItemView itemView = e.ItemPresenter.GetComponentInChildren<ProjectItemView>(true);
                itemView.ProjectItem = null;
            }
            else
            {
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = projectItem.Name;
                ProjectItemView itemView = e.ItemPresenter.GetComponentInChildren<ProjectItemView>(true);
                itemView.ProjectItem = projectItem;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            if (!m_previewsCreated)
            {
                return;
            }

            AssetItem assetItem = (AssetItem)e.NewItem;
            HandleSelectionChanged(assetItem);
        }

        private void HandleSelectionChanged(AssetItem assetItem)
        {
            if (assetItem != null && assetItem.TypeGuid == m_noneGuid)
            {
                IsNoneSelected = true;
                SelectedObject = null;
            }
            else
            {
                IsNoneSelected = false;

                if (assetItem != null)
                {
                    if (m_sceneObjects.ContainsKey(assetItem.ItemID))
                    {
                        SelectedObject = m_sceneObjects[assetItem.ItemID];
                    }
                    else
                    {
                        SelectedObject = null;
                        m_project.Load(new[] { assetItem }, (error, obj) =>
                        {
                            SelectedObject = obj[0];
                        });
                    }
                }
                else
                {
                    SelectedObject = null;
                }
            }

            m_parentDialog.IsOkInteractable = m_treeView.SelectedItem != null;
        }

        private void OnItemDoubleClick(object sender, ItemArgs e)
        {
            AssetItem assetItem = (AssetItem)e.Items[0];
            if (assetItem != null && assetItem.TypeGuid == m_noneGuid)
            {
                IsNoneSelected = true;
                SelectedObject = null;
                m_parentDialog.Close(true);
            }
            else
            {
                IsNoneSelected = false;
                if (assetItem != null)
                {
                    if(m_sceneObjects.ContainsKey(assetItem.ItemID))
                    {
                        SelectedObject = m_sceneObjects[assetItem.ItemID];
                        m_parentDialog.Close(true);
                    }
                    else
                    {
                        SelectedObject = null;
                        Editor.IsBusy = true;
                        m_project.Load(new[] { assetItem }, (error, obj) =>
                        {
                            Editor.IsBusy = false;
                            SelectedObject = obj[0];
                            m_parentDialog.Close(true);
                        });
                    }   
                }
                else
                {
                    SelectedObject = null;
                    m_parentDialog.Close(true);
                }
            }
        }

        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            if (SelectedObject == null && !IsNoneSelected)
            {
                args.Cancel = true;
            }
        }

        private void OnFilterValueChanged(string text)
        {
            ApplyFilter(text);
        }

        private void ApplyFilter(string text)
        {
            if (m_coApplyFilter != null)
            {
                StopCoroutine(m_coApplyFilter);
            }
            StartCoroutine(m_coApplyFilter = CoApplyFilter(text));
        }

        private IEnumerator m_coApplyFilter;
        private IEnumerator CoApplyFilter(string filter)
        {
            yield return new WaitForSeconds(0.3f);

            ApplyFilterImmediately(filter);
        }

        private void ApplyFilterImmediately(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                m_treeView.Items = m_toggleAssets.isOn ? m_assetsCache : m_sceneCache;
            }
            else
            {
                AssetItem[] cache = m_toggleAssets.isOn ? m_assetsCache : m_sceneCache;
                m_treeView.Items = cache.Where(item => item.Name.ToLower().Contains(filter.ToLower()));
            }
        }

        private void OnAssetsTabSelectionChanged(bool value)
        {
            ApplyFilterImmediately(m_filter.text);
        }
    }
}

