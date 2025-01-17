﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Utilities.Editor
{
	public static class TypeExtensions
	{
		#region Attributes

		public static bool HasAttribute<TAttributeType>(this Type type) where TAttributeType : Attribute
		{
			return type.GetAttribute<TAttributeType>() != null;
		}

		public static TAttributeType GetAttribute<TAttributeType>(this Type type) where TAttributeType : Attribute
		{
			return type.TryGetAttribute<TAttributeType>(out var attribute) ? attribute : null;
		}

		public static bool TryGetAttribute<TAttributeType>(this Type type, out TAttributeType attribute) where TAttributeType : Attribute
		{
			var attributes = type.GetCustomAttributes(typeof(TAttributeType), false);
			attribute = attributes != null && attributes.Length > 0 ? attributes[0] as TAttributeType : null;

			return attribute != null;
		}

		public static bool HasAttribute(this Type type, Type attributeType)
		{
			return type.GetAttribute(attributeType) != null;
		}

		public static Attribute GetAttribute(this Type type, Type attributeType)
		{
			return type.TryGetAttribute(attributeType, out var attribute) ? attribute : null;
		}

		public static bool TryGetAttribute(this Type type, Type attributeType, out Attribute attribute)
		{
			var attributes = type.GetCustomAttributes(attributeType, false);
			attribute = attributes != null && attributes.Length > 0 ? attributes[0] as Attribute : null;

			return attribute != null;
		}

		#endregion

		#region Hierarchy

		public static T CreateInstance<T>(this Type type) where T : class
		{
			if (type.IsCreatableAs(typeof(T)))
            {
                return Activator.CreateInstance(type) as T;
            }

            return null;
		}

		public static bool IsCreatable(this Type type)
		{
			return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
		}

		public static bool IsCreatableAs<TBaseType>(this Type type)
		{
			return type.IsCreatableAs(typeof(TBaseType));
		}

		public static bool IsCreatableAs(this Type type, Type baseType)
		{
			return baseType.IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null;
		}

		public static bool InheritsGeneric(this Type type, Type genericType)
		{
			var baseType = type.BaseType;

			while (baseType != null)
			{
				if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }

                baseType = baseType.BaseType;
			}

			return false;
		}

		public static IEnumerable<Type> GetDerivedTypes(this Type baseType, bool includeAbstract)
		{
			var types = TypeCache.GetTypesDerivedFrom(baseType).Prepend(baseType);
			return includeAbstract ? types : types.Where(type => !type.IsAbstract);
		}

		public static bool ImplementsInterface<TInterfaceType>(this Type type)
		{
			return type.ImplementsInterface(typeof(TInterfaceType));
		}

		public static bool ImplementsInterface(this Type type, Type interfaceType)
		{
			var interfaces = type.GetInterfaces();
			return interfaces != null && interfaces.Contains(interfaceType);
		}

		#endregion

		#region Serialization

		// these functions are based on Unity's serialization rules defined here: https://docs.unity3d.com/Manual/script-Serialization.html

		public static List<Type> SerializableTypes = new List<Type>
		{
			typeof(bool),
			typeof(sbyte), typeof(short), typeof(int), typeof(long),
			typeof(byte), typeof(ushort), typeof(uint), typeof(ulong),
			typeof(float), typeof(double), typeof(decimal),
			typeof(char), typeof(string),
			typeof(Vector2), typeof(Vector3), typeof(Vector4),
			typeof(Quaternion), typeof(Matrix4x4),
			typeof(Color), typeof(Color32), typeof(Gradient),
			typeof(Rect), typeof(RectOffset),
			typeof(LayerMask), typeof(AnimationCurve), typeof(GUIStyle)
		};

		public static bool IsSerializable(this Type type)
		{
			return type.IsSerializable(false);
		}

		private static bool IsSerializable(this Type type, bool inner)
		{
			if (type.IsAbstract)
            {
                return false; // covers static as well
            }

            if (type.IsEnum)
            {
                return true;
            }

            if (type.IsGenericType)
            {
                return !inner && type.GetGenericTypeDefinition() == typeof(List<>) && IsSerializable(type.GetGenericArguments()[0], true);
            }

            if (type.IsArray && type.GetElementType().IsSerializable(true))
            {
                return !inner;
            }

            if (typeof(Object).IsAssignableFrom(type))
            {
                return true;
            }

            if (type.HasAttribute<SerializableAttribute>())
            {
                return true;
            }

            return SerializableTypes.Contains(type);
		}

		#endregion
	}
}