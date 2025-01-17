﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
#endif
	public partial class IconButton : Image
	{
		#region Log Messages

		private const string MISSING_ICON_WARNING = "(PUEIBMI) unknown icon '{0}' for IconButton: the icon could not be found";

		#endregion 

		#region Class Names

		public const string STYLESHEET = "IconButton.uss";
		public const string USS_CLASS_NAME = "pirho-icon-button";

		#endregion

		#region Private Members

		private Clickable _clickable;

		#endregion

		#region Public Interface

		public event Action Clicked
		{
			add
			{
				if (_clickable == null)
				{
					_clickable = new Clickable(value);
					this.AddManipulator(_clickable);
				}
				else
				{
					_clickable.clicked += value;
				}
			}
			remove
			{
				if (_clickable != null)
                {
                    _clickable.clicked -= value;
                }
            }
		}

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("icon")]
        private string IconName
        {
            get => image == null || string.IsNullOrEmpty(image.name) ? string.Empty : image.name;
            set => SetIcon(value);
        }
#endif

		public IconButton() : this(null)
		{
		}

		public IconButton(Action clickEvent)
		{
			Clicked += clickEvent;

			AddToClassList(USS_CLASS_NAME);
			this.AddStyleSheet(STYLESHEET);
		}

		public void SetIcon(string iconName)
		{
            if (string.IsNullOrEmpty(iconName))
            {
                image = null;
                
                return;
            }
            
			if (_icons.TryGetValue(iconName, out var icon))
            {
                image = icon.Texture;
            }
            else
            {
                Debug.LogWarningFormat(MISSING_ICON_WARNING, iconName);
            }
        }

		#endregion

		#region UXML Support

		private static readonly Dictionary<string, Icon> _icons = new Dictionary<string, Icon>
		{
			{ "Add", Icon.Add },
			{ "CustomAdd", Icon.CustomAdd },
			{ "Remove", Icon.Remove },
			{ "Inspect", Icon.Inspect },
			{ "Expanded", Icon.Expanded },
			{ "Collapsed", Icon.Collapsed },
			{ "Refresh", Icon.Refresh },
			{ "Load", Icon.Load },
			{ "Unload", Icon.Unload },
			{ "Close", Icon.Close },
			{ "LeftArrow", Icon.LeftArrow },
			{ "RightArrow", Icon.RightArrow },
			{ "Info", Icon.Info },
			{ "Warning", Icon.Warning },
			{ "Error", Icon.Error },
			{ "Settings", Icon.Settings },
			{ "View", Icon.View },
			{ "Locked", Icon.Locked },
			{ "Unlocked", Icon.Unlocked }
		};

#if !UNITY_2023_2_OR_NEWER
		public new class UxmlFactory : UxmlFactory<IconButton, UxmlTraits> { }
		public new class UxmlTraits : Image.UxmlTraits
		{
			private readonly UxmlStringAttributeDescription _icon = new UxmlStringAttributeDescription { name = "icon", use = UxmlAttributeDescription.Use.Required };

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);

				var button = (IconButton)ve;
				var name = _icon.GetValueFromBag(bag, cc);

				button.SetIcon(name);
			}
		}
#endif

		#endregion
	}
}
