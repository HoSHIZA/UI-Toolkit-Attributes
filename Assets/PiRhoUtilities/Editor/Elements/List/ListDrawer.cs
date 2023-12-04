using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(ListAttribute))]
    internal class ListDrawer : PropertyDrawer
	{
		private const string INVALID_TYPE_WARNING = "(PULDIT) invalid type for ListAttribute on field '{0}': List can only be applied to SerializedList or SerializedArray fields";
		private const string INVALID_ADD_CALLBACK_WARNING = "(PULDIAC) invalid add callback for ListAttribute on field '{0}': The method must accept an int or have no parameters";
		private const string INVALID_ADD_REFERENCE_CALLBACK_WARNING = "(PULDIAC) invalid add callback for ListAttribute on field '{0}': The method must accept an int and/or an object in either order or have no parameters";
		private const string INVALID_REMOVE_CALLBACK_WARNING = "(PULDIRMC) invalid remove callback for ListAttribute on field '{0}': The method must accept an int or have no parameters";
		private const string INVALID_REORDER_CALLBACK_WARNING = "(PULDIROC) invalid reorder callback for ListAttribute on field '{0}': The method must accept two ints or have no parameters";
		private const string INVALID_CHANGE_CALLBACK_WARNING = "(PULDICC) invalid change callback for ListAttribute on field '{0}': The method must have no parameters";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var items = property.FindPropertyRelative(SerializedList<string>.ITEMS_PROPERTY);

			if (items != null && items.isArray)
			{
				var isReference = fieldInfo.FieldType.BaseType.GetGenericTypeDefinition() == typeof(ReferenceList<>);
				var referenceType = isReference ? fieldInfo.GetFieldType() : null;
				var declaringType = fieldInfo.DeclaringType;
				var listAttribute = attribute as ListAttribute;
				var drawer = this.GetNextDrawer();
				var proxy = new PropertyListProxy(items, drawer);

				var field = new ListField
				{
					IsCollapsable = listAttribute.IsCollapsable,
					bindingPath = items.propertyPath,
					Label = property.displayName
				};

				// TODO: other stuff from ConfigureField

				if (!string.IsNullOrEmpty(listAttribute.EmptyLabel))
                {
                    field.EmptyLabel = listAttribute.EmptyLabel;
                }

                field.AllowAdd = listAttribute.AllowAdd != ListAttribute.NEVER;
				field.AllowRemove = listAttribute.AllowRemove != ListAttribute.NEVER;
				field.AllowReorder = listAttribute.AllowReorder;

				SetupAdd(listAttribute, proxy, field, property, declaringType, isReference);
				SetupRemove(listAttribute, proxy, field, property, declaringType);
				SetupReorder(listAttribute, field, property, declaringType);
				SetupChange(listAttribute, field, property, declaringType);

				field.SetProxy(proxy, referenceType, true);

				return field;
			}
			else
			{
				Debug.LogWarningFormat(INVALID_TYPE_WARNING, property.propertyPath);
				return new FieldContainer(property.displayName, string.Empty);
			}
		}

		private void SetupAdd(ListAttribute listAttribute, PropertyListProxy proxy, ListField field, SerializedProperty property, Type declaringType, bool isReference)
		{
			if (field.AllowAdd)
			{
				if (!string.IsNullOrEmpty(listAttribute.AllowAdd))
                {
                    proxy.CanAddCallback = ReflectionHelper.CreateValueSourceFunction(listAttribute.AllowAdd, property, field, declaringType, true);
                }

                if (!string.IsNullOrEmpty(listAttribute.AddCallback))
				{
					if (!isReference)
					{
						var addCallback = ReflectionHelper.CreateActionCallback(listAttribute.AddCallback, declaringType, property);
						if (addCallback != null)
						{
							field.RegisterCallback<ListField.ItemAddedEvent>(evt => addCallback.Invoke());
						}
						else
						{
							var addCallbackIndex = ReflectionHelper.CreateActionCallback<int>(listAttribute.AddCallback, declaringType, property);
							if (addCallbackIndex != null)
                            {
                                field.RegisterCallback<ListField.ItemAddedEvent>(evt => addCallbackIndex.Invoke(evt.Index));
                            }
                            else
                            {
                                Debug.LogWarningFormat(INVALID_ADD_CALLBACK_WARNING, property.propertyPath);
                            }
                        }
					}
					else
					{
						var addCallback = ReflectionHelper.CreateActionCallback(listAttribute.AddCallback, declaringType, property);
						if (addCallback != null)
						{
							field.RegisterCallback<ListField.ItemAddedEvent>(evt => addCallback.Invoke());
						}
						else
						{
							var addCallbackIndex = ReflectionHelper.CreateActionCallback<int>(listAttribute.AddCallback, declaringType, property);
							if (addCallbackIndex != null)
                            {
                                field.RegisterCallback<ListField.ItemAddedEvent>(evt => addCallbackIndex.Invoke(evt.Index));
                            }
                            else
                            {
                                Debug.LogWarningFormat(INVALID_ADD_REFERENCE_CALLBACK_WARNING, property.propertyPath);
                            }
                        }
					}
				}
			}
		}

		private void SetupRemove(ListAttribute listAttribute, PropertyListProxy proxy, ListField field, SerializedProperty property, Type declaringType)
		{
			if (field.AllowRemove)
			{
				if (!string.IsNullOrEmpty(listAttribute.AllowRemove))
				{
					proxy.CanRemoveCallback = ReflectionHelper.CreateFunctionCallback<int, bool>(listAttribute.AllowRemove, declaringType, property);

					if (proxy.CanRemoveCallback == null)
					{
						var canRemove = ReflectionHelper.CreateValueSourceFunction(listAttribute.AllowRemove, property, field, declaringType, true);
						proxy.CanRemoveCallback = index => canRemove();
					}
				}

				if (!string.IsNullOrEmpty(listAttribute.RemoveCallback))
				{
					var removeCallback = ReflectionHelper.CreateActionCallback(listAttribute.RemoveCallback, declaringType, property);
					if (removeCallback != null)
					{
						field.RegisterCallback<ListField.ItemRemovedEvent>(evt => removeCallback.Invoke());
					}
					else
					{
						var removeCallbackIndex = ReflectionHelper.CreateActionCallback<int>(listAttribute.RemoveCallback, declaringType, property);
						if (removeCallbackIndex != null)
                        {
                            field.RegisterCallback<ListField.ItemRemovedEvent>(evt => removeCallbackIndex.Invoke(evt.Index));
                        }
                        else
                        {
                            Debug.LogWarningFormat(INVALID_REMOVE_CALLBACK_WARNING, property.propertyPath);
                        }
                    }
				}
			}
		}

		private void SetupReorder(ListAttribute listAttribute, ListField field, SerializedProperty property, Type declaringType)
		{
			if (field.AllowReorder)
			{
				if (!string.IsNullOrEmpty(listAttribute.ReorderCallback))
				{
					var reorderCallback = ReflectionHelper.CreateActionCallback(listAttribute.ReorderCallback, declaringType, property);
					if (reorderCallback != null)
					{
						field.RegisterCallback<ListField.ItemReorderedEvent>(evt => reorderCallback.Invoke());
					}
					else
					{
						var reorderCallbackFromTo = ReflectionHelper.CreateActionCallback<int, int>(listAttribute.ReorderCallback, declaringType, property);
						if (reorderCallbackFromTo != null)
                        {
                            field.RegisterCallback<ListField.ItemReorderedEvent>(evt => reorderCallbackFromTo.Invoke(evt.FromIndex, evt.ToIndex));
                        }
                        else
                        {
                            Debug.LogWarningFormat(INVALID_REORDER_CALLBACK_WARNING, property.propertyPath);
                        }
                    }
				}
			}
		}

		private void SetupChange(ListAttribute listAttribute, ListField field, SerializedProperty property, Type declaringType)
		{
			if (!string.IsNullOrEmpty(listAttribute.ChangeCallback))
			{
				var changeCallback = ReflectionHelper.CreateActionCallback(listAttribute.ChangeCallback, declaringType, property);
				if (changeCallback != null)
                {
                    field.RegisterCallback<ListField.ItemsChangedEvent>(evt => changeCallback.Invoke());
                }
                else
                {
                    Debug.LogWarningFormat(INVALID_CHANGE_CALLBACK_WARNING, property.propertyPath);
                }
            }
		}
	}
}
