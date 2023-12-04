using System;
using System.Reflection;

namespace PiRhoSoft.Utilities
{
	public static class InternalHelper
	{
		// TODO: these are all static only right now - probably need the instance type in the signature for non statics
		// TODO: need to be able to pass in parameter types to disambiguate overloads
		// TODO: error reporting

		public static TDelegateType CreateDelegate<TDelegateType>(MethodInfo method) where TDelegateType : Delegate
		{
			return (TDelegateType)Delegate.CreateDelegate(typeof(TDelegateType), method, false);
		}

		public static TDelegateType CreateDelegate<TDelegateType>(Type type, string methodName) where TDelegateType : Delegate
		{
			var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			return method != null ? (TDelegateType)Delegate.CreateDelegate(typeof(TDelegateType), method, false) : null;
		}

		public static Func<TPropertyType> CreateGetDelegate<TPropertyType>(Type type, string propertyName)
		{
			var property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			var method = property != null ? property.GetGetMethod(true) : null; // nonPublic parameter means also get non public, rather than only get non public

			return method != null ? (Func<TPropertyType>)Delegate.CreateDelegate(typeof(Func<TPropertyType>), method, false) : null;
		}

		public static Action<TPropertyType> CreateSetDelegate<TPropertyType>(Type type, string propertyName)
		{
			var property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			var method = property != null ? property.GetSetMethod(true) : null; // same as GetGetMethod in CreateGetDelegate

			return method != null ? (Action<TPropertyType>)Delegate.CreateDelegate(typeof(Action<TPropertyType>), method, false) : null;
		}

		public static Func<TFieldType> CreateGetField<TFieldType>(Type type, string fieldName)
		{
			var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			if (field != null)
				return () => (TFieldType)field.GetValue(null);

			return null;
		}

		public static Action<TFieldType> CreateSetField<TFieldType>(Type type, string fieldName)
		{
			var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			if (field != null)
				return value => field.SetValue(null, value);

			return null;
		}

		public static Func<TFieldType> CreateGetField<TFieldType>(Type type, string fieldName, object obj)
		{
			var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			if (field != null)
				return () => (TFieldType)field.GetValue(obj);

			return null;
		}

		public static Action<TFieldType> CreateSetField<TFieldType>(Type type, string fieldName, object obj)
		{
			var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			if (field != null)
				return value => field.SetValue(obj, value);

			return null;
		}
	}
}
