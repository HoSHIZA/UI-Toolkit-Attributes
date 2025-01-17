﻿using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
#endif
	public partial class Frame : BindableElement, INotifyValueChanged<bool>
	{
		#region Class Names

		public const string STYLESHEET = "Frame.uss";
		public const string USS_CLASS_NAME = "pirho-frame";
		public const string HEADER_USS_CLASS_NAME = USS_CLASS_NAME + "__header";
		public const string LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label";
		public const string NO_LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label--none";
		public const string CONTENT_USS_CLASS_NAME = USS_CLASS_NAME + "__content";
		public const string HEADER_BUTTONS_USS_CLASS_NAME = USS_CLASS_NAME + "__header-buttons";
		public const string HEADER_BUTTON_USS_CLASS_NAME = USS_CLASS_NAME + "__header-button";
		public const string COLLAPSE_BUTTON_USS_CLASS_NAME = USS_CLASS_NAME + "__collapse-button";
		public const string COLLAPSABLE_USS_CLASS_NAME = USS_CLASS_NAME + "--collapsable";
		public const string EXPANDED_USS_CLASS_NAME = USS_CLASS_NAME + "--expanded";
		public const string COLLAPSED_USS_CLASS_NAME = USS_CLASS_NAME + "--collapsed";

		#endregion

		#region Defaults
        
        public const string COLLAPSE_TOOLTIP = "Collapse this view";
        public const string EXPAND_TOOLTIP = "Expand this view";

		public const bool DEFAULT_IS_COLLAPSABLE = true;
		public const bool DEFAULT_IS_COLLAPSED = false;

		#endregion

		#region Icons

		public static readonly Icon ExpandIcon = Icon.Collapsed;
		public static readonly Icon CollapseIcon = Icon.Expanded;

		#endregion

		#region Private Members

		private Label _labelElement;

		private readonly bool _addChildren = true;

		private bool _isCollapsable = DEFAULT_IS_COLLAPSABLE;
		private bool _isCollapsed = DEFAULT_IS_COLLAPSED;
		private string _label = null;
		private string _tooltip = null;

		#endregion

		#region Public Interface

		public VisualElement Header { get; private set; }
		public VisualElement Content { get; private set; }

        public IconButton CollapseButton { get; private set; }

		public VisualElement HeaderButtons { get; private set; }
        
		public Frame()
		{
			BuildUi();
		}

		protected Frame(bool addChildren) : this()
		{
			_addChildren = addChildren;
		}
        
#if UNITY_2023_2_OR_NEWER
        [Header("Frame")]
        [UxmlAttribute("label")]
#endif
        public string Label
        {
            get => _label;
            set => SetLabel(value);
        }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("tooltip")]
#endif
        public string Tooltip
        {
            get => _tooltip;
            set => SetTooltip(value);
        }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("is-collapsable")]
#endif
		public bool IsCollapsable
		{
			get => _isCollapsable;
			set => SetCollapsable(value);
		}

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("is-collapsed")]
#endif
		public bool IsCollapsed
		{
			get => _isCollapsed;
			set => SetCollapsed(value);
		}

		public IconButton AddHeaderButton(Texture icon, string tooltip, string ussClassName, Action action)
		{
			var button = new IconButton(action) { image = icon, tooltip = tooltip };
			button.AddToClassList(HEADER_BUTTON_USS_CLASS_NAME);

			if (!string.IsNullOrEmpty(ussClassName))
            {
                button.AddToClassList(ussClassName);
            }

            HeaderButtons.Add(button);
			return button;
		}

		#endregion

		#region State Setters

		private void SetCollapsable(bool isCollapsable)
		{
			if (_isCollapsable != isCollapsable)
			{
				_isCollapsable = isCollapsable;
				UpdateCollapse();
			}
		}

		private void SetCollapsed(bool isCollapsed)
		{
			var previous = _isCollapsed;

			if (isCollapsed != previous)
			{
				SetCollapsedWithoutNotify(isCollapsed);
				this.SendChangeEvent(previous, isCollapsed);
			}
		}

		private void SetCollapsedWithoutNotify(bool isCollapsed)
		{
			_isCollapsed = isCollapsed;
			UpdateCollapse();
		}

		private void SetLabel(string label)
		{
			_label = label;
			UpdateLabel();
		}

		private void SetTooltip(string tooltip)
		{
			_tooltip = tooltip;
			UpdateLabel();
		}

		#endregion

		#region UI

		public override VisualElement contentContainer => Content;

		private void BuildUi()
		{
			AddToClassList(USS_CLASS_NAME);
			this.AddStyleSheet(STYLESHEET);

			Header = new VisualElement();
			Header.AddToClassList(HEADER_USS_CLASS_NAME);
			hierarchy.Add(Header);

            CollapseButton = new IconButton(() => IsCollapsed = !IsCollapsed) { image = CollapseIcon.Texture, tooltip = COLLAPSE_TOOLTIP };
			CollapseButton.AddToClassList(COLLAPSE_BUTTON_USS_CLASS_NAME);
			Header.Add(CollapseButton);
            
			_labelElement = new Label();
			_labelElement.AddToClassList(LABEL_USS_CLASS_NAME);
			Header.Add(_labelElement);

			HeaderButtons = new VisualElement();
			HeaderButtons.AddToClassList(HEADER_BUTTONS_USS_CLASS_NAME);
			Header.Add(HeaderButtons);

			Content = new VisualElement();
			Content.AddToClassList(CONTENT_USS_CLASS_NAME);
            Content.name = "content";
			hierarchy.Add(Content);

			UpdateCollapse();
			UpdateLabel();
            
#if UNITY_2021_1_OR_NEWER
            style.marginRight = -2;
#else
            style.marginRight = 3;
#endif
            
#if !UNITY_2020_1_OR_NEWER
            {
                style.backgroundColor = StyleConst.UnityColors.Window.Background;

                var border = StyleConst.UnityColors.Window.Border;
                style.borderLeftColor = border;
                style.borderTopColor = border;
                style.borderRightColor = border;
                style.borderBottomColor = border;
            }

            {
                Header.style.backgroundColor = StyleConst.UnityColors.Default.Background;
                
                _labelElement.style.color = StyleConst.UnityColors.Default.Text;
                _collapseButton.tintColor = StyleConst.UnityColors.Button.Text.value;
            }

            {
                var buttons = HeaderButtons.Query(className: HEADER_BUTTON_USS_CLASS_NAME);

                foreach (var button in buttons.Build())
                {
                    button.style.color = StyleConst.UnityColors.Button.Background;
                }
            }
#endif
		}

		private void UpdateCollapse()
		{
			EnableInClassList(COLLAPSABLE_USS_CLASS_NAME, _isCollapsable);
			EnableInClassList(EXPANDED_USS_CLASS_NAME, !_isCollapsed);
			EnableInClassList(COLLAPSED_USS_CLASS_NAME, _isCollapsed);

            CollapseButton.image = _isCollapsed ? ExpandIcon.Texture : CollapseIcon.Texture;
			CollapseButton.tooltip = _isCollapsed ? EXPAND_TOOLTIP : COLLAPSE_TOOLTIP;
		}

		private void UpdateLabel()
		{
			_labelElement.EnableInClassList(NO_LABEL_USS_CLASS_NAME, string.IsNullOrEmpty(_label));
			_labelElement.text = _label;
			_labelElement.tooltip = _tooltip;
		}

		#endregion

		#region Binding

		bool INotifyValueChanged<bool>.value
		{
			get => !IsCollapsed;
			set => IsCollapsed = !value;
		}

		void INotifyValueChanged<bool>.SetValueWithoutNotify(bool newValue)
		{
			SetCollapsedWithoutNotify(!newValue);
		}

#if UNITY_2023_2_OR_NEWER
		protected override void HandleEventBubbleUp(EventBase evt)
		{
			base.HandleEventBubbleUp(evt);
#else
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
#endif
            
			if (this.TryGetPropertyBindEvent(evt, out var property))
			{
				// TODO: Support binding expanded state directly to a bool?
                
				BindingExtensions.CreateBind(this, property, GetExpandedProperty, SetExpandedProperty, CompareExpandedProperties);

				Label =_label ?? property.displayName;
				Tooltip = _tooltip ?? property.GetTooltip();
                
				if (_addChildren && property.HasVisibleChildFields())
				{
					Content.Clear();

					foreach (var child in property.Children())
					{
						var field = new PropertyField(child);
						Content.Add(field);
					}
				}

				evt.StopPropagation();
			}
		}

		private static bool GetExpandedProperty(SerializedProperty property)
		{
			return property.isExpanded;
		}

		private static void SetExpandedProperty(SerializedProperty property, bool value)
		{
			property.isExpanded = value;
		}

		private static bool CompareExpandedProperties(bool value, SerializedProperty property, Func<SerializedProperty, bool> getter)
		{
			var currentValue = getter(property);
			return value == currentValue;
		}

		#endregion

		#region UXML

#if !UNITY_2023_2_OR_NEWER
		public new class UxmlFactory : UxmlFactory<Frame, UxmlTraits> { }
		public new class UxmlTraits : BindableElement.UxmlTraits
		{
			private readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription { name = "label" };
			private readonly UxmlStringAttributeDescription _tooltip = new UxmlStringAttributeDescription { name = "tooltip" };
			private readonly UxmlBoolAttributeDescription _collapsable = new UxmlBoolAttributeDescription { name = "is-collapsable", defaultValue = DEFAULT_IS_COLLAPSABLE };
			private readonly UxmlBoolAttributeDescription _collapsed = new UxmlBoolAttributeDescription { name = "is-collapsed", defaultValue = DEFAULT_IS_COLLAPSED };

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);

				var frame = (Frame)ve;
				frame.Label = _label.GetValueFromBag(bag, cc);
				frame.Tooltip = _tooltip.GetValueFromBag(bag, cc);
				frame.IsCollapsable = _collapsable.GetValueFromBag(bag, cc);
				frame.IsCollapsed = _collapsed.GetValueFromBag(bag, cc);

				// TODO: Figure out how to support header buttons
			}
		}
#endif

		#endregion
	}
}
