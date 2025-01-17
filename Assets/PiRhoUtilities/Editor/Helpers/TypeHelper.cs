﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Utilities.Editor
{
	public class TypeList
	{
		public TypeList(Type baseType)
		{
			BaseType = baseType;
		}

		public Type BaseType { get; private set; }

		public List<Type> Types;
		public List<string> Paths;
	}

	public static class TypeHelper
	{
		private static Dictionary<string, TypeList> _derivedTypeLists = new Dictionary<string, TypeList>();

		#region Listing

		public static IEnumerable<Type> GetDerivedTypes<TBaseType>(bool includeAbstract)
		{
			return typeof(TBaseType).GetDerivedTypes(includeAbstract);
		}

		public static IEnumerable<Type> GetTypesWithAttribute<TAttributeType>() where TAttributeType : Attribute
		{
			return TypeCache.GetTypesWithAttribute<TAttributeType>();
		}

		public static IEnumerable<Type> GetTypesWithAttribute(Type attributeType)
		{
			return TypeCache.GetTypesWithAttribute(attributeType);
		}

		public static TypeList GetTypeList<T>(bool includeAbstract)
		{
			return GetTypeList(typeof(T), includeAbstract);
		}

		public static TypeList GetTypeList(Type baseType, bool includeAbstract)
		{
			// include the settings in the name so lists of the same type can be created with different settings
			var listName = string.Format("{0}-{1}", includeAbstract, baseType.AssemblyQualifiedName);

			if (!_derivedTypeLists.TryGetValue(listName, out var typeList))
			{
				typeList = new TypeList(baseType);
				_derivedTypeLists.Add(listName, typeList);
			}

			if (typeList.Types == null)
			{
				var types = baseType.GetDerivedTypes(includeAbstract);
				var ordered = types.Select(type => new PathedType(types, baseType, type)).OrderBy(type => type.Path);

				typeList.Types = ordered.Select(type => type.Type).ToList();
				typeList.Paths = ordered.Select(type => type.Path).ToList();
			}

			return typeList;
		}

		private class PathedType
		{
			public Type Type;
			public string Path;

			public PathedType(IEnumerable<Type> types, Type rootType, Type type)
			{
				Type = type;
				Path = Type.Name;

				// repeat the name for types that have derivations so they appear in their own submenu (otherwise they wouldn't be selectable)
				if (type != rootType)
				{
					if (types.Any(t => t.BaseType == type))
                    {
                        Path += "/" + Type.Name;
                    }

                    type = type.BaseType;
				}

				// prepend all parent type names up to but not including the root type
				while (type != rootType && type != typeof(object)) // check against object in case rootType is an interface
				{
					Path = type.Name + "/" + Path;
					type = type.BaseType;
				}
			}
		}

		#endregion

		#region Utility

		public static Type FindType(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            
			// search with normal rules
			var type = Type.GetType(name, false);

			// search in default runtime assembly
			if (type == null)
            {
                type = Type.GetType($"{name}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false);
            }

            // search in default editor assembly
			if (type == null)
            {
                type = Type.GetType($"{name}, Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false);
            }

            // search in Unity
			if (type == null)
            {
                type = typeof(Object).Assembly.GetType(name, false);
            }

            return type;
		}

		#endregion
	}
}
