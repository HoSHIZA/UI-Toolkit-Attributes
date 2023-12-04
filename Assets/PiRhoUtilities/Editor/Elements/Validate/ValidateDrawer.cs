using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(ValidateAttribute))]
    internal class ValidateDrawer : PropertyDrawer
	{
		public const string STYLESHEET = "ValidateStyle.uss";
		public const string USS_CLASS_NAME = "pirho-validate";
		public const string SIDE_USS_CLASS_NAME = USS_CLASS_NAME + "--side";
		public const string MESSAGE_USS_CLASS_NAME = USS_CLASS_NAME + "__message";
		public const string ABOVE_USS_CLASS_NAME = MESSAGE_USS_CLASS_NAME + "--above";
		public const string BELOW_USS_CLASS_NAME = MESSAGE_USS_CLASS_NAME + "--below";
		public const string LEFT_USS_CLASS_NAME = MESSAGE_USS_CLASS_NAME + "--left";
		public const string RIGHT_USS_CLASS_NAME = MESSAGE_USS_CLASS_NAME + "--right";

		private const string INVALID_TYPE_WARNING = "(PUVDIT) invalid type for ValidateAttribute on field '{0}': Validate can only be applied to serializable fields";
		private const string INVALID_METHOD_WARNING = "(PUVDIM) invalid method for ValidateAttribute on field '{0}': the method '{1}' should return a bool and take 0, or 1 parameter of type '{2}'";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var element = this.CreateNextElement(property);
			var validateAttribute = attribute as ValidateAttribute;

			var message = new MessageBox(validateAttribute.Type, validateAttribute.Message);
			message.AddToClassList(MESSAGE_USS_CLASS_NAME);

			var change = CreateControl(message, property, fieldInfo.DeclaringType, validateAttribute.Method);
			if (change != null)
			{
				var container = new VisualElement();
				container.AddToClassList(USS_CLASS_NAME);
				container.AddStyleSheet(STYLESHEET);
				container.Add(element);
				container.Add(change);

				if (validateAttribute.Location == TraitLocation.Above)
				{
					message.AddToClassList(BELOW_USS_CLASS_NAME);
					container.Insert(0, message);
				}
				if (validateAttribute.Location == TraitLocation.Below)
				{
					message.AddToClassList(BELOW_USS_CLASS_NAME);
					container.Add(message);
				}
				else if (validateAttribute.Location == TraitLocation.Left)
				{
					message.AddToClassList(LEFT_USS_CLASS_NAME);
					container.Insert(0, message);
					container.AddToClassList(SIDE_USS_CLASS_NAME);
				}
				else if (validateAttribute.Location == TraitLocation.Right)
				{
					message.AddToClassList(RIGHT_USS_CLASS_NAME);
					container.Add(message);
					container.AddToClassList(SIDE_USS_CLASS_NAME);
				}

				return container;
			}

			return element;
		}
		
		private PropertyWatcher CreateControl(MessageBox message, SerializedProperty property, Type declaringType, string method)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Integer: return CreateControl<int>(message, property, declaringType, method);
				case SerializedPropertyType.Boolean: return CreateControl<bool>(message, property, declaringType, method);
				case SerializedPropertyType.Float: return CreateControl<float>(message, property, declaringType, method);
				case SerializedPropertyType.String: return CreateControl<string>(message, property, declaringType, method);
				case SerializedPropertyType.Color: return CreateControl<Color>(message, property, declaringType, method);
				case SerializedPropertyType.ObjectReference: return CreateControl<Object>(message, property, declaringType, method);
				case SerializedPropertyType.LayerMask: return CreateControl<int>(message, property, declaringType, method);
				case SerializedPropertyType.Enum: return CreateControl<Enum>(message, property, declaringType, method);
				case SerializedPropertyType.Vector2: return CreateControl<Vector2>(message, property, declaringType, method);
				case SerializedPropertyType.Vector2Int: return CreateControl<Vector2Int>(message, property, declaringType, method);
				case SerializedPropertyType.Vector3: return CreateControl<Vector3>(message, property, declaringType, method);
				case SerializedPropertyType.Vector3Int: return CreateControl<Vector3Int>(message, property, declaringType, method);
				case SerializedPropertyType.Vector4: return CreateControl<Vector4>(message, property, declaringType, method);
				case SerializedPropertyType.Rect: return CreateControl<Rect>(message, property, declaringType, method);
				case SerializedPropertyType.RectInt: return CreateControl<RectInt>(message, property, declaringType, method);
				case SerializedPropertyType.Bounds: return CreateControl<Bounds>(message, property, declaringType, method);
				case SerializedPropertyType.BoundsInt: return CreateControl<BoundsInt>(message, property, declaringType, method);
				case SerializedPropertyType.Character: return CreateControl<char>(message, property, declaringType, method);
				case SerializedPropertyType.AnimationCurve: return CreateControl<AnimationCurve>(message, property, declaringType, method);
				case SerializedPropertyType.Gradient: return CreateControl<Gradient>(message, property, declaringType, method);
				case SerializedPropertyType.Quaternion: return CreateControl<Quaternion>(message, property, declaringType, method);
				case SerializedPropertyType.ExposedReference: return CreateControl<Object>(message, property, declaringType, method);
				case SerializedPropertyType.FixedBufferSize: return CreateControl<int>(message, property, declaringType, method);
				case SerializedPropertyType.ManagedReference: return CreateControl<object>(message, property, declaringType, method);
			}

			Debug.LogWarningFormat(INVALID_TYPE_WARNING, property.propertyPath, this.GetFieldType().Name);
			return null;
		}

		private PropertyWatcher CreateControl<T>(MessageBox message, SerializedProperty property, Type declaringType, string method)
		{
			var none = ReflectionHelper.CreateFunctionCallback<bool>(method, declaringType, property);
			if (none != null)
			{
				Validated(message, none());
				return new ChangeTrigger<T>(property, (_, oldValue, newValue) => Validated(message, none()));
			}
			else
			{
				var one = ReflectionHelper.CreateFunctionCallback<T, bool>(method, declaringType, property);
				if (one != null)
				{
					var change = new ChangeTrigger<T>(property, (_, oldValue, newValue) => Validated(message, one(newValue)));
					Validated(message, one(change.value));
					return change;
				}
			}

			Debug.LogWarningFormat(INVALID_METHOD_WARNING, property.propertyPath, method, typeof(T).Name);
			return null;
		}

		private void Validated(MessageBox message, bool valid)
		{
			if (!EditorApplication.isPlaying)
            {
                message.SetDisplayed(!valid);
            }
        }
	}
}