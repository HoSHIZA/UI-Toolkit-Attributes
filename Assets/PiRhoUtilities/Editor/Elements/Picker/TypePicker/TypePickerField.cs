using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Utilities.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement("TypePicker")]
#endif
	public partial class TypePickerField : PickerField<string>
	{
		#region Class Names

		public new const string STYLESHEET = "Picker/TypePicker/TypePickerStyle.uss";
		public new const string USS_CLASS_NAME = "pirho-scene-picker-field";

		#endregion

		#region Log Messages

		private const string INVALID_TYPE_WARNING = "(PUTPFIT) Invalid type for TypePickerField: the type '{0}' could not be found";
		private const string INVALID_VALUE_WARNING = "(PUTPFIT) Failed to set TypePickerField value: '{0}' is not derivable from type '{1}'";

		#endregion

		#region Defaults

		public const bool DEFAULT_SHOW_ABSTRACT = false;

		#endregion

		#region Members

		private TypePickerControl Picker => Control as TypePickerControl;

		#endregion

		#region Public Interface

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("type")]
#endif
		public Type Type
		{
			get => Picker.Type;
			set => Picker.SetType(value, ShowAbstract);
		}

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("show-abstract")]
#endif
		public bool ShowAbstract
		{
			get => Picker.ShowAbstract;
			set => Picker.SetType(Type, value);
		}

		public TypePickerField() : this(null, null)
		{
		}

		public TypePickerField(string label) : base(label, new TypePickerControl())
		{
			AddToClassList(USS_CLASS_NAME);
		}

		public TypePickerField(string label, Type type, bool showAbstract = DEFAULT_SHOW_ABSTRACT) : this(label)
		{
			Picker.SetType(type, showAbstract);
		}

		public TypePickerField(Type type, bool showAbstract = DEFAULT_SHOW_ABSTRACT) : this(null, type, showAbstract)
		{
		}

		#endregion

		#region Visual Input

		private class TypeProvider : PickerProvider<string> { }
		private class TypePickerControl : PickerControl, IDragReceiver
		{
			public Type Type { get; private set; }
			public bool ShowAbstract { get; private set; } = DEFAULT_SHOW_ABSTRACT;

			private Type _value;
			private string ValueName => _value?.AssemblyQualifiedName ?? string.Empty;

			private Texture _typeIcon;

			public TypePickerControl()
			{
				SetLabel(null, GetLabel());

				this.MakeDragReceiver();
			}

			public override void SetValueWithoutNotify(string newValue)
			{
				if (ValueName != newValue)
				{
					var type = GetType(newValue);

					if (!string.IsNullOrEmpty(newValue) && (Type == null || !Type.IsAssignableFrom(type)))
					{
						Debug.LogWarningFormat(INVALID_VALUE_WARNING, newValue, Type);
					}
					else
					{
						_value = type;

						var icon = GetIcon(type);
						var text = GetLabel();

						SetLabel(icon, text);
					}
				}
			}

			public void SetType(Type type, bool showAbstract)
			{
				if (type != Type || showAbstract != ShowAbstract)
				{
					if (Provider)
                    {
                        Object.DestroyImmediate(Provider);
                    }

                    Provider = null;

					Type = type;
					ShowAbstract = showAbstract;

					if (Type == null)
					{
						Debug.LogWarningFormat(INVALID_TYPE_WARNING);
					}
					else
					{
						var types = TypeHelper.GetTypeList(Type, showAbstract);

						Provider = ScriptableObject.CreateInstance<TypeProvider>();
						Provider.Setup(types.BaseType.Name, types.Paths.Prepend("None").ToList(), types.Types.Select(t => t.AssemblyQualifiedName).Prepend(string.Empty).ToList(), GetIcon, OnSelected);
						_typeIcon = AssetPreview.GetMiniTypeThumbnail(Type);
					}

					if (Type == null || (_value != null && !Type.IsAssignableFrom(_value)))
                    {
                        OnSelected(null);
                    }
                    else
                    {
                        SetLabel(GetIcon(Type), GetLabel());
                    }
                }
			}

			private Type GetType(string typeName)
			{
				return string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName, false);
			}

			private Texture GetIcon(string typeName)
			{
				var type = GetType(typeName);
				return GetIcon(type);
			}

			private Texture GetIcon(Type type)
			{
				if (type == null)
                {
                    return _typeIcon;
                }

                var icon = AssetPreview.GetMiniTypeThumbnail(type);
				
				if (icon == null)
                {
                    return _typeIcon;
                }

                return icon;
			}

			private string GetLabel()
			{
				return _value == null ? $"None ({Type?.Name ?? "No Base Type"})" : _value.Name;
			}

			private void OnSelected(string selected)
			{
				if (ValueName != selected)
                {
                    this.SendChangeEvent(ValueName, selected);
                }
            }

			#region IDragReceiver Implementation

			public bool IsDragValid(Object[] objects, object data)
			{
				if (objects.Length > 0)
				{
					var obj = objects[0];
					if (obj != null)
					{
						var drag = obj.GetType();
						return Type != null && Type.IsAssignableFrom(drag);
					}
				}

				return false;
			}

			public void AcceptDrag(Object[] objects, object data)
			{
				OnSelected(objects[0].GetType().AssemblyQualifiedName);
			}

			#endregion
		}

		#endregion

		#region UXML Support

#if !UNITY_2023_2_OR_NEWER
		public new class UxmlFactory : UxmlFactory<TypePickerField, UxmlTraits> { }
		public new class UxmlTraits : BaseFieldTraits<string, UxmlStringAttributeDescription>
		{
			private readonly UxmlStringAttributeDescription _type = new UxmlStringAttributeDescription { name = "type" };
			private readonly UxmlBoolAttributeDescription _showAbstract = new UxmlBoolAttributeDescription { name = "show-abstract", defaultValue = DEFAULT_SHOW_ABSTRACT };

			public override void Init(VisualElement element, IUxmlAttributes bag, CreationContext cc)
			{
				var field = element as TypePickerField;
				var typeName = _type.GetValueFromBag(bag, cc);

				if (!string.IsNullOrEmpty(typeName))
                {
                    field.Type = TypeHelper.FindType(typeName);
                }

                field.ShowAbstract = _showAbstract.GetValueFromBag(bag, cc);

				base.Init(element, bag, cc);
			}
		}
#endif

		#endregion
	}
}
