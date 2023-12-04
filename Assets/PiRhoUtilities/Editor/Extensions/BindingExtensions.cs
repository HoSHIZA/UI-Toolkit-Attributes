using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
    public static class BindingExtensions
    {
        #region Internal Lookups

        private const string CHANGED_INTERNALS_ERROR = "(PUBFECI) failed to setup BindingExtensions: Unity internals have changed";
        
        private const string BINDING_NAMESPACE = "UnityEditor.UIElements.Bindings";

        private const string SERIALIZED_OBJECT_BINDING_CONTEXT_TYPE_NAME = BINDING_NAMESPACE + ".SerializedObjectBindingContext, UnityEditor";

        private static readonly Type _serializedObjectBindingContextType;

        private const string DEFAULT_BIND_NAME = "DefaultBind";
        private static readonly MethodInfo _defaultBindEnumMethod;

        private const string SERIALIZED_OBJECT_BINDING_TYPE_NAME = BINDING_NAMESPACE + ".SerializedObjectBinding`1, UnityEditor";
        private static readonly Type _serializedObjectBindingType;

        private const string CREATE_BIND_NAME = "CreateBind";
        private static readonly Dictionary<Type, MethodInfo> _createBindMethods = new Dictionary<Type, MethodInfo>();

        static BindingExtensions()
        {
            var serializedObjectBindingContextType = Type.GetType(SERIALIZED_OBJECT_BINDING_CONTEXT_TYPE_NAME);
            var serializedObjectBindingContextConstructor = serializedObjectBindingContextType?.GetConstructor(new Type[]
            {
                typeof(SerializedObject)
            });
            
            if (serializedObjectBindingContextConstructor != null)
            {
                _serializedObjectBindingContextType = serializedObjectBindingContextType;
            }

            var defaultBindMethod = serializedObjectBindingContextType?.GetMethod(DEFAULT_BIND_NAME, 
                BindingFlags.Instance | BindingFlags.NonPublic);
            var defaultBindEnumMethod = defaultBindMethod?.MakeGenericMethod(typeof(Enum));

            if (defaultBindEnumMethod != null && defaultBindEnumMethod.HasSignature(null,
                    typeof(VisualElement),
                    typeof(SerializedProperty),
                    typeof(Func<SerializedProperty, Enum>),
                    typeof(Action<SerializedProperty, Enum>),
                    typeof(Func<Enum, SerializedProperty, Func<SerializedProperty, Enum>, bool>)))
            {
                _defaultBindEnumMethod = defaultBindEnumMethod;
            }

            _serializedObjectBindingType = Type.GetType(SERIALIZED_OBJECT_BINDING_TYPE_NAME);

            if (_serializedObjectBindingContextType == null || _defaultBindEnumMethod == null || _serializedObjectBindingType == null)
            {
                Debug.LogError(CHANGED_INTERNALS_ERROR);
            }
        }

        #endregion

        #region Helper Methods

        public static void CreateBind<TValue>(INotifyValueChanged<TValue> field, SerializedProperty property, 
            Func<SerializedProperty, TValue> getter, 
            Action<SerializedProperty, TValue> setter, 
            Func<TValue, SerializedProperty, Func<SerializedProperty, TValue>, bool> comparer)
        {
            if (!_createBindMethods.TryGetValue(typeof(TValue), out var createBindMethod))
            {
                var serializedObjectBindingType = _serializedObjectBindingType?.MakeGenericType(typeof(TValue));
                createBindMethod = serializedObjectBindingType?.GetMethod(CREATE_BIND_NAME, BindingFlags.Public | BindingFlags.Static);

                _createBindMethods.Add(typeof(TValue), createBindMethod);
            }

            var context = Activator.CreateInstance(_serializedObjectBindingContextType, property.serializedObject);

            createBindMethod?.Invoke(null, new object[]
            {
                field, context, property, getter, setter, comparer
            });
        }

        public static void DefaultEnumBind(INotifyValueChanged<Enum> field, SerializedProperty property)
        {
            var type = field.value.GetType();
            var wrapper = Activator.CreateInstance(_serializedObjectBindingContextType, property.serializedObject);

            _defaultBindEnumMethod.Invoke(wrapper, new object[]
            {
                field, property, (Func<SerializedProperty, Enum>)Getter,
                (Action<SerializedProperty, Enum>)Setter,
                (Func<Enum, SerializedProperty, Func<SerializedProperty, Enum>, bool>)Comparer
            });

            return;

            Enum Getter(SerializedProperty p)
            {
                return Enum.ToObject(type, p.intValue) as Enum;
            }

            void Setter(SerializedProperty p, Enum v)
            {
                p.intValue = (int)Enum.Parse(type, v.ToString());
            }

            bool Comparer(Enum v, SerializedProperty p, Func<SerializedProperty, Enum> g)
            {
                return g(p).Equals(v);
            }
        }

        public static void BindManagedReference<TReferenceType>(INotifyValueChanged<TReferenceType> field, SerializedProperty property,
            Action onSet)
        {
            CreateBind(field, property, GetManagedReference<TReferenceType>, (p, v) =>
            {
                SetManagedReference(p, v);
                onSet?.Invoke();
            }, CompareManagedReferences);
        }

        private static TReferenceType GetManagedReference<TReferenceType>(SerializedProperty property)
        {
            var value = property.GetManagedReferenceValue();
            if (value is TReferenceType reference)
            {
                return reference;
            }

            return default;
        }

        private static void SetManagedReference<TReferenceType>(SerializedProperty property, TReferenceType value)
        {
            // PENDING: built in change tracking for undo doesn't work for ManagedReference yet

            Undo.RegisterCompleteObjectUndo(property.serializedObject.targetObject, "Change reference");

            property.managedReferenceValue = value;

            property.serializedObject.ApplyModifiedProperties();
            Undo.FlushUndoRecordObjects();
        }

        private static bool CompareManagedReferences<TReferenceType>(TReferenceType value, SerializedProperty property,
            Func<SerializedProperty, TReferenceType> getter)
        {
            var currentValue = getter(property);

            return ReferenceEquals(value, currentValue);
        }

        #endregion
    }
}