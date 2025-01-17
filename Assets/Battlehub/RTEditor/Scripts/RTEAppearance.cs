﻿using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public interface IRTEAppearance
    {
        RTECursor[] CursorSettings
        {
            get;
            set;
        }

        RTEColors Colors
        {
            get;
            set;
        }

        [Obsolete("Use UIScaler instead")]
        CanvasScaler UIBackgroundScaler
        {
            get;
        }

        [Obsolete("Use UIScaler instead")]
        CanvasScaler UIForegroundScaler
        {
            get;
        }

        CanvasScaler UIScaler
        {
            get;
        }

        void ApplyColors(GameObject root);
    }

    [Serializable]
    public struct RTECursor
    {
        public string Name;
        public KnownCursor Type;
        public Texture2D Texture;
    }

    [Serializable]
    public struct RTESelectableColors
    {
        public Color Normal;
        public Color Highlight;
        public Color Pressed;
        public Color Disabled;
        public Color Selected;

        public RTESelectableColors(Color normal, Color highlight, Color pressed, Color disabled)
        {
            Normal = normal;
            Highlight = highlight;
            Pressed = pressed;
            Disabled = disabled;
            Selected = new Color32(0, 0x97, 0xFF, 0xFF);
        }

        public RTESelectableColors(Color normal, Color highlight, Color pressed, Color disabled, Color32 selected)
        {
            Normal = normal;
            Highlight = highlight;
            Pressed = pressed;
            Disabled = disabled;
            Selected = selected;
        }


        public bool EqualTo(RTESelectableColors c)
        {
            return
                c.Normal == Normal &&
                c.Highlight == Highlight &&
                c.Pressed == Pressed &&
                c.Disabled == Disabled &&
                c.Selected == Selected;
        }
    }

    [Serializable]
    public struct RTEHierarchyColors
    {
        public Color NormalItem;
        public Color DisabledItem;
        
        public RTEHierarchyColors(Color normal, Color disabled)
        {
            NormalItem = normal;
            DisabledItem = disabled;
        }

        public bool EqualTo(RTEHierarchyColors c)
        {
            return
                c.NormalItem == NormalItem &&
                c.DisabledItem == DisabledItem;
        }
    }

    [Serializable]
    public struct RTEMenuItemColors
    {
        public Color SelectionColor;
        public Color TextColor;
        public Color DisabledSelectionColor;
        public Color DisabledTextColor;

        public RTEMenuItemColors(Color selectionColor, Color textColor, Color disabledSelectionColor, Color disabledTextColor)
        {
            SelectionColor = selectionColor;
            TextColor = textColor;
            DisabledSelectionColor = disabledSelectionColor;
            DisabledTextColor = disabledTextColor;
        }

        public bool EqualTo(RTEMenuItemColors c)
        {
            return
                c.SelectionColor == SelectionColor &&
                c.TextColor == TextColor &&
                c.DisabledSelectionColor == DisabledSelectionColor &&
                c.DisabledTextColor == DisabledTextColor;
        }
    }

    [Serializable]
    public class RTEColors
    {
        public static readonly Color DefaultPrimary = new Color32(0x38, 0x38, 0x38, 0xFF);
        public static readonly Color DefaultSecondary = new Color32(0x27, 0x27, 0x27, 0xFF);
        public static readonly Color DefaultBorder = new Color32(0x20, 0x20, 0x20, 0xFF);
        public static readonly Color DefaultBorder2 = new Color32(0x1A, 0x1A, 0x1A, 0xFF);
        public static readonly Color DefaultBorder3 = new Color32(0x52, 0x4C, 0x4C, 0xFF);
        public static readonly Color DefaultBorder4 = new Color(0x87, 0x87, 0x87, 0xFF);
        public static readonly Color DefaultAccent = new Color32(0x0, 0x97, 0xFF, 0xC0);
        public static readonly Color DefaultText = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        public static readonly Color DefaultText2 = new Color32(0xFF, 0xFF, 0xFF, 0x7F);
        public static readonly Color DefaultModalOverlay = new Color32(0x00, 0x00, 0x00, 0x40);
        public static readonly Color DefaultMainMenuBar = new Color32(0x6F, 0x6F, 0x6F, 0xFF);
        public static readonly Color DefaultToolsToggle = new Color32(0xC8, 0xC8, 0xC8, 0x7F);
        public static readonly Color DefaultFooter = new Color32(0x6F, 0x6F, 0x6F, 0xFF);
        public static readonly RTESelectableColors DefaultMainMenuButton = new RTESelectableColors(new Color32(0xff, 0xff, 0xff, 0x00), new Color32(0x0, 0x97, 0xFF, 0x7F), new Color32(0x0, 0x97, 0xFF, 0xFF), new Color32(0, 0, 0, 0));
        public static readonly RTEMenuItemColors DefaultMenuItem = new RTEMenuItemColors(new Color32(0x00, 0x97, 0xFF, 0xFF), new Color32(0xFF, 0xFF, 0xFF, 0xFF), new Color32(0x95, 0x93, 0x93, 0x7F), new Color32(0x71, 0x71, 0x71, 0xFF));
        public static readonly RTEHierarchyColors DefaultHierarchy = new RTEHierarchyColors(Color.white, new Color32(0x93, 0x92, 0x92, 0xFF));
        public static readonly Color DefaultProjectFolder = new Color32(0x48, 0x48, 0x48, 0xFF);
        public static readonly RTESelectableColors DefaultConsoleButton = new RTESelectableColors(DefaultPrimary, DefaultPrimary, DefaultSecondary, new Color32(0xC8, 0xC8, 0xC8, 0x7F));
        public static readonly RTESelectableColors DefaultScrollBar = new RTESelectableColors(DefaultPrimary, DefaultPrimary, DefaultSecondary, new Color32(0xC8, 0xC8, 0xC8, 0x7F));
        public static readonly Color DefaultScrollBarBackground = new Color32(0xB2, 0xB2, 0xB2, 0xFF);
        public static readonly RTESelectableColors DefaultToggle = new RTESelectableColors(DefaultSecondary, DefaultSecondary, new Color32(0x20, 0x20, 0x20, 0xFF), new Color32(0x38, 0x38, 0x38, 0x7F));
        public static readonly RTESelectableColors DefaultToggleButton = new RTESelectableColors(new Color32(0x44, 0x44, 0x44, 0xFF), new Color32(0x38, 0x38, 0x38, 0xFF), new Color32(0x27, 0x27, 0x27, 0xFF), new Color32(0xC8, 0xC8, 0xC8, 0x7F));
        public static readonly RTESelectableColors DefaultInputField = new RTESelectableColors(DefaultSecondary, DefaultSecondary, new Color32(0x20, 0x20, 0x20, 0xFF), new Color32(0x5C, 0x5C, 0x5C, 0x7F), new Color32(0, 0x97, 0xFF, 0xFF));
        public static readonly RTESelectableColors DefaultInputField2 = new RTESelectableColors(DefaultSecondary, DefaultSecondary, new Color32(0x20, 0x20, 0x20, 0xFF), new Color32(0x5C, 0x5C, 0x5C, 0x7F), new Color32(0x20, 0x20, 0x20, 0xFF));
        public static readonly RTESelectableColors DefaultButton = new RTESelectableColors(new Color32(0x44, 0x44, 0x44, 0xFF), new Color32(0x55, 0x55, 0x55, 0xFF), new Color32(0x38, 0x38, 0x38, 0x7F), new Color32(0x5C, 0x5C, 0x5C, 0x66));
        public static readonly RTESelectableColors DefaultButton2 = new RTESelectableColors(new Color(0xFF, 0xFF, 0xFF, 0xFF), new Color32(0xF5, 0xF5, 0xF5, 0xFF), new Color32(0xC8, 0xC8, 0xC8, 0xFF), new Color32(0xC8, 0xC8, 0xC8, 0x7F));
        public static readonly RTESelectableColors DefaultSlider = new RTESelectableColors(new Color32(0x44, 0x44, 0x44, 0xFF), new Color32(0x55, 0x55, 0x55, 0xFF), new Color32(0x38, 0x38, 0x38, 0x7F), new Color32(0x5C, 0x5C, 0x5C, 0x66));
        public static readonly RTESelectableColors DefaultDropdown = new RTESelectableColors(new Color32(0x44, 0x44, 0x44, 0xFF), new Color32(0x55, 0x55, 0x55, 0xFF), new Color32(0x38, 0x38, 0x38, 0x7F), new Color32(0x5C, 0x5C, 0x5C, 0x66));

        public Color Primary;
        public Color Secondary;
        public Color Border;
        public Color Border2;
        public Color Border3;
        public Color Border4;
        public Color Accent;
        public Color Text;
        public Color Text2;
        public Color ModalOverlay;
        public Color MainMenuBar;
        public Color Footer;
        public RTESelectableColors MainMenuButton;
        public Color ToolsToggle;
        public RTEMenuItemColors MenuItem;
        public RTEHierarchyColors Hierarchy;
        public Color ProjectFolder;
        public RTESelectableColors ConsoleButton;
        public RTESelectableColors ScrollBar;
        public Color ScrollBarBackground;
        public RTESelectableColors Toggle;
        public RTESelectableColors ToggleButton;
        public RTESelectableColors InputField;
        public RTESelectableColors InputField2;
        public RTESelectableColors Button;
        public RTESelectableColors Button2;
        public RTESelectableColors Slider;
        public RTESelectableColors Dropdown;
        
        public RTEColors()
        {
            Primary = DefaultPrimary;
            Secondary = DefaultSecondary;
            Border = DefaultBorder;
            Border2 = DefaultBorder2;
            Border3 = DefaultBorder3;
            Border4 = DefaultBorder4;
            Accent = DefaultAccent;
            Text = DefaultText;
            Text2 = DefaultText2;
            ModalOverlay = DefaultModalOverlay;
            MainMenuBar = DefaultMainMenuBar;
            ToolsToggle = DefaultToolsToggle;
            Footer = DefaultFooter;
            MainMenuButton = DefaultMainMenuButton;
            MenuItem = DefaultMenuItem;
            Hierarchy = DefaultHierarchy;
            ProjectFolder = DefaultProjectFolder;
            ConsoleButton = DefaultConsoleButton;
            ScrollBar = DefaultScrollBar;
            ScrollBarBackground = DefaultScrollBarBackground;
            Toggle = DefaultToggle;
            ToggleButton = DefaultToggleButton;
            InputField = DefaultInputField;
            InputField2 = DefaultInputField2;
            Button = DefaultButton;
            Button2 = DefaultButton2;
            Slider = DefaultSlider;
            Dropdown = DefaultDropdown;
        }

        public bool IsDefault
        {
            get
            {
                return
                    Primary == DefaultPrimary &&
                    Secondary == DefaultSecondary &&
                    Border == DefaultBorder &&
                    Border2 == DefaultBorder2 &&
                    Border3 == DefaultBorder3 &&
                    Border4 == DefaultBorder4 &&
                    Accent == DefaultAccent &&
                    Text == DefaultText &&
                    Text2 == DefaultText2 &&
                    ModalOverlay == DefaultModalOverlay &&
                    MainMenuBar == DefaultMainMenuBar &&
                    ToolsToggle == DefaultToolsToggle &&
                    Footer == DefaultFooter &&
                    MainMenuButton.EqualTo(DefaultMainMenuButton) &&
                    MenuItem.EqualTo(DefaultMenuItem) &&
                    Hierarchy.EqualTo(DefaultHierarchy) &&
                    ProjectFolder == DefaultProjectFolder &&
                    ConsoleButton.EqualTo(DefaultConsoleButton) &&
                    ScrollBar.EqualTo(DefaultScrollBar) &&
                    ScrollBarBackground == DefaultScrollBarBackground &&
                    Toggle.EqualTo(DefaultToggle) &&
                    ToggleButton.EqualTo(DefaultToggleButton) &&
                    InputField.EqualTo(DefaultInputField) &&
                    InputField2.EqualTo(DefaultInputField2) &&
                    Button.EqualTo(DefaultButton) &&
                    Button2.EqualTo(DefaultButton2) &&
                    Slider.EqualTo(DefaultSlider) &&
                    Dropdown.EqualTo(DefaultDropdown);
                    
            }
        }
    }

    [DefaultExecutionOrder(-90)]
    public class RTEAppearance : MonoBehaviour, IRTEAppearance
    {
        [SerializeField]
        private RTECursor[] m_cursorSettings = null;
        public RTECursor[] CursorSettings
        {
            get { return m_cursorSettings; }
            set
            {
                m_cursorSettings = value;
                if (m_editor != null && m_editor.IsOpened)
                {
                    ApplyCursorSettings();
                }
            }
        }

        [SerializeField]
        private CanvasScaler m_uiBackgroundScaler = null;
        [Obsolete("Use UIScaler instead")]
        public CanvasScaler UIBackgroundScaler
        {
            get { return m_uiBackgroundScaler; }
        }

        [SerializeField]
        private CanvasScaler m_uiForegroundScaler = null;
        [Obsolete("Use UIScaler instead")]
        public CanvasScaler UIForegroundScaler
        {
            get { return m_uiForegroundScaler; }
        }

        [SerializeField]
        private CanvasScaler m_uiScaler = null;
        public CanvasScaler UIScaler
        {
            get { return m_uiScaler; }
        }

        [SerializeField]
        private RTEColors m_colors = null;
        public RTEColors Colors
        {
            get { return m_colors; }
            set
            {
                m_colors = value;
                ApplyColors(gameObject);
            }
        }

        [SerializeField]
        private GameObject[] m_prefabs = null;

        private IRTE m_editor;
        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();

            List<RTECursor> cursorSettings;
            if (m_cursorSettings == null)
            {
                cursorSettings = new List<RTECursor>();
            }
            else
            {
                cursorSettings = m_cursorSettings.ToList();
            }

            AddCursorIfRequired(cursorSettings, KnownCursor.DropAllowed, "Drag & Drop Allowed", "RTE_DropAllowed_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.DropNotAllowed, "Drag & Drop Not Allowed", "RTE_DropNotAllowed_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.HResize, "Horizontal Resize", "RTE_HResize_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.VResize, "Vertical Resize", "RTE_VResize_Cursor");
            m_cursorSettings = cursorSettings.ToArray();

            m_editor.IsOpenedChanged += OnIsOpenedChanged;
            if (m_editor.IsOpened)
            {
                ApplyCursorSettings();
                ApplyColors();
            }
        }

        private static void AddCursorIfRequired(List<RTECursor> cursorSettings, KnownCursor cursorType, string name, string texture)
        {
            if (!cursorSettings.Any(c => c.Type == cursorType))
            {
                RTECursor dropAllowedCursor = new RTECursor
                {
                    Name = name,
                    Type = cursorType,
                    Texture = Resources.Load<Texture2D>(texture)
                };
                cursorSettings.Add(dropAllowedCursor);
            }
        }

        private void OnDestroy()
        {
            if (m_editor != null)
            {
                m_editor.IsOpenedChanged -= OnIsOpenedChanged;
            }
        }

        private void OnIsOpenedChanged()
        {
            if (m_editor.IsOpened)
            {
                ApplyCursorSettings();
                ApplyColors();
            }
        }

        private void ApplyCursorSettings()
        {
            for (int i = 0; i < m_cursorSettings.Length; ++i)
            {
                RTECursor cursor = m_cursorSettings[i];
                m_editor.CursorHelper.Map(cursor.Type, cursor.Texture);
            }
        }

        private void ApplyColors()
        {
            if (Colors.IsDefault)
            {
                Debug.Log("Using default colors");
                return;
            }
            ApplyColors(gameObject);
        }

        public void ApplyColors(GameObject root)
        {
            UIStyle[] styles = root.GetComponentsInChildren<UIStyle>(true)
                .Union(m_prefabs.Where(p => p != null).SelectMany(p => p.GetComponentsInChildren<UIStyle>(true))).ToArray();
            for(int i = 0; i < styles.Length; ++i)
            {
                UIStyle style = styles[i];
                switch(style.Name)
                {
                    case "PrimaryColor":
                        style.ApplyImageColor(Colors.Primary);
                        break;
                    case "SecondaryColor":
                        style.ApplyImageColor(Colors.Secondary);
                        break;
                    case "BorderColor":
                        style.ApplyImageColor(Colors.Border);
                        break;
                    case "Border2Color":
                        style.ApplyImageColor(Colors.Border2);
                        break;
                    case "Border3Color":
                        style.ApplyImageColor(Colors.Border3);
                        style.ApplyOutlineColor(Colors.Border3);
                        break;
                    case "Border4Color":
                        style.ApplyImageColor(Colors.Border4);
                        style.ApplyOutlineColor(Colors.Border4);
                        break;
                    case "AccentColor":
                        style.ApplyImageColor(Colors.Accent);
                        break;
                    case "TextColor":
                        style.ApplyTextColor(Colors.Text);
                        style.ApplyImageColor(Colors.Text);
                        break;
                    case "Text2Color":
                        style.ApplyTextColor(Colors.Text2);
                        style.ApplyImageColor(Colors.Text2);
                        break;
                    case "ModalOverlayColor":
                        style.ApplyImageColor(Colors.ModalOverlay);
                        break;
                    case "MainMenuBarColor":
                        style.ApplyImageColor(Colors.MainMenuBar);
                        break;
                    case "MainMenuButtonColor":
                        style.ApplyMainButtonColor(Colors.MainMenuButton.Normal, Colors.MainMenuButton.Highlight, Colors.MainMenuButton.Pressed);
                        break;
                    case "MenuItemColor":
                        style.ApplyMenuItemColor(Colors.MenuItem.SelectionColor, Colors.MenuItem.TextColor, Colors.MenuItem.DisabledSelectionColor, Colors.MenuItem.DisabledTextColor);
                        break;
                    case "ToolsToggleColor":
                        style.ApplyImageColor(Colors.ToolsToggle);
                        break;
                    case "FooterColor":
                        style.ApplyImageColor(Colors.Footer);
                        break;
                    case "HierarchyColor":
                        style.ApplyHierarchyColors(Colors.Hierarchy.NormalItem, Colors.Hierarchy.DisabledItem);
                        break;
                    case "ProjectFolderColor":
                        style.ApplyImageColor(Colors.ProjectFolder);
                        break;
                    case "ConsoleButtonColor":
                        style.ApplySelectableColor (Colors.ConsoleButton.Normal, Colors.ConsoleButton.Highlight, Colors.ConsoleButton.Pressed, Colors.ConsoleButton.Disabled);
                        break;
                    case "ScrollBarColor":
                        style.ApplySelectableColor(Colors.ScrollBar.Normal, Colors.ScrollBar.Highlight, Colors.ScrollBar.Pressed, Colors.ScrollBar.Disabled);
                        break;
                    case "ScrollBarBackgroundColor":
                        style.ApplyImageColor(Colors.ScrollBarBackground);
                        break;
                    case "ToggleColor":
                        style.ApplySelectableColor(Colors.Toggle.Normal, Colors.Toggle.Highlight, Colors.Toggle.Pressed, Colors.Toggle.Disabled);
                        break;
                    case "ToggleButtonColor":
                        style.ApplySelectableColor(Colors.ToggleButton.Normal, Colors.ToggleButton.Highlight, Colors.ToggleButton.Pressed, Colors.ToggleButton.Disabled);
                        break;
                    case "InputFieldColor":
                        style.ApplyInputFieldColor(Colors.InputField.Normal, Colors.InputField.Highlight, Colors.InputField.Pressed, Colors.InputField.Disabled, Colors.InputField.Selected);
                        break;
                    case "InputField2Color":
                        style.ApplyInputFieldColor(Colors.InputField2.Normal, Colors.InputField2.Highlight, Colors.InputField2.Pressed, Colors.InputField2.Disabled, Colors.InputField2.Selected);
                        break;
                    case "ButtonColor":
                        style.ApplySelectableColor(Colors.Button.Normal, Colors.Button.Highlight, Colors.Button.Pressed, Colors.Button.Disabled);
                        break;
                    case "Button2Color":
                        style.ApplySelectableColor(Colors.Button2.Normal, Colors.Button2.Highlight, Colors.Button2.Pressed, Colors.Button2.Disabled);
                        break;
                    case "SliderColor":
                        style.ApplySelectableColor(Colors.Slider.Normal, Colors.Slider.Highlight, Colors.Slider.Pressed, Colors.Slider.Disabled);
                        break;
                    case "DropdownColor":
                        style.ApplySelectableColor(Colors.Dropdown.Normal, Colors.Dropdown.Highlight, Colors.Dropdown.Pressed, Colors.Dropdown.Disabled);
                        break;
                }
            }
        }
    }

}

