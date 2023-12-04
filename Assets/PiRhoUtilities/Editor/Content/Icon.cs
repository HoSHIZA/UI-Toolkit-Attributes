using System;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.Utilities.Editor
{
	public class Icon
	{
		public static readonly Icon Add = BuiltIn("Toolbar Plus");
		public static readonly Icon CustomAdd = BuiltIn("Toolbar Plus More");
		public static readonly Icon Remove = BuiltIn("Toolbar Minus");
		public static readonly Icon Inspect = BuiltIn("UnityEditor.InspectorWindow");
		public static readonly Icon Expanded = BuiltIn("IN foldout on");
		public static readonly Icon Collapsed = BuiltIn("IN foldout");
		public static readonly Icon Refresh = BuiltIn("preAudioLoopOff");
		public static readonly Icon Load = BuiltIn("SceneLoadIn");
		public static readonly Icon Unload = BuiltIn("SceneLoadOut");
		public static readonly Icon Close = BuiltIn("winbtn_win_close");
		public static readonly Icon LeftArrow = BuiltIn("tab_prev");
		public static readonly Icon RightArrow = BuiltIn("tab_next");
		public static readonly Icon Info = BuiltIn("console.infoicon");
		public static readonly Icon Warning = BuiltIn("console.warnicon");
		public static readonly Icon Error = BuiltIn("console.erroricon");
		public static readonly Icon Settings = BuiltIn("_Popup");
		public static readonly Icon View = BuiltIn("ViewToolOrbit");
		public static readonly Icon SearchBarLeft = BuiltIn("toolbarsearch");
		public static readonly Icon SearchBarRightCancel = BuiltIn("toolbarsearchCancelButton");
		public static readonly Icon SearchBarRight = BuiltIn("toolbarsearchCancelButtonOff");
		public static readonly Icon Locked = BuiltIn("LockIcon-On");
		public static readonly Icon Unlocked = BuiltIn("LockIcon");

		private const string INVALID_ICON_ERROR = "(PUIIII) failed to create icon texture: the built in icon {0} could not be loaded";
		private const string INVALID_DATA_ERROR = "(PUIIID) failed to create icon texture: the supplied data is not a valid base 64 string";
		private const string INVALID_TEXTURE_ERROR = "(PUIIIT) failed to create icon texture: the supplied data is not a valid texture";

		public static Icon BuiltIn(string name) => new Icon { _name = name };
		public static Icon Base64(string data) => new Icon { _data = data };

		private string _name;
		private string _data;

		private Texture _texture;

		public Texture Texture
		{
			get
			{
				if (_texture == null)
                {
                    _texture = LoadTexture();
                }

                return _texture;
			}
		}

		private Icon() { }

		private Texture LoadTexture()
		{
			if (!string.IsNullOrEmpty(_name))
			{
				var content = EditorGUIUtility.IconContent(_name);

				if (content != null)
                {
                    return content.image;
                }
                else
                {
                    Debug.LogErrorFormat(INVALID_ICON_ERROR, _name);
                }
            }
			else if (!string.IsNullOrEmpty(_data))
			{
				var content = new Texture2D(1, 1);
				var data = Convert.FromBase64String(_data);

				content.hideFlags = HideFlags.HideAndDontSave;

				if (data != null)
				{
					if (content.LoadImage(data))
                    {
                        return content;
                    }
                    else
                    {
                        Debug.LogError(INVALID_TEXTURE_ERROR);
                    }
                }
				else
				{
					Debug.LogError(INVALID_DATA_ERROR);
				}
			}

			return EditorGUIUtility.whiteTexture;
		}
	}
}
