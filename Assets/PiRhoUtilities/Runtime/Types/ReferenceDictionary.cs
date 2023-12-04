using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiRhoSoft.Utilities
{
	[Serializable]
	public class ReferenceDictionary<TKeyType, TValueType> : Dictionary<TKeyType, TValueType>, ISerializationCallbackReceiver
	{
		public const string KEY_PROPERTY = nameof(_keys);
		public const string VALUE_PROPERTY = nameof(_values);

		// These are protected so they can be found by the editor.
		[SerializeField] protected List<TKeyType> _keys = new List<TKeyType>();
		[SerializeReference] protected List<TValueType> _values = new List<TValueType>();

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			ConvertToLists();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			ConvertFromLists();
		}

		private void ConvertToLists()
		{
			_keys.Clear();
			_values.Clear();

			foreach (var entry in this)
			{
				_keys.Add(entry.Key);
				_values.Add(entry.Value);
			}
		}

		private void ConvertFromLists()
		{
			Clear();

			var count = Math.Min(_keys.Count, _values.Count);

			for (var i = 0; i < count; i++)
            {
                Add(_keys[i], _values[i]);
            }
        }
	}
}
