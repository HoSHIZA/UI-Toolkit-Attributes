using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[InitializeOnLoad]
	public class HistoryWindow : EditorWindow
	{
		#region Class Names

		public const string STYLESHEET = "HistoryStyle.uss";
		public const string USS_CLASS_NAME = "pirho-history";
		public const string HEADER_USS_CLASS_NAME = USS_CLASS_NAME + "__header";
		public const string HEADER_BUTTON_USS_CLASS_NAME = HEADER_USS_CLASS_NAME + "__button";
		public const string LIST_USS_CLASS_NAME = USS_CLASS_NAME + "__list";
		public const string LIST_ITEM_USS_CLASS_NAME = LIST_USS_CLASS_NAME + "__item";
		public const string LIST_ITEM_ICON_USS_CLASS_NAME = LIST_ITEM_USS_CLASS_NAME + "__icon";
		public const string LIST_ITEM_LABEL_USS_CLASS_NAME = LIST_ITEM_USS_CLASS_NAME + "__label";
		public const string CURRENT_LIST_ITEM_USS_CLASS_NAME = LIST_ITEM_USS_CLASS_NAME + "--current";

		#endregion

		#region Window Names

		private const string WINDOW_MENU = "Window/PiRho Utilities/History";
		private const string MOVE_BACK_MENU = "Edit/History/Move Back &LEFT";
		private const string MOVE_FORWARD_MENU = "Edit/History/Move Forward &RIGHT";

		#endregion

		#region Icons

		private static readonly Icon _icon = Icon.BuiltIn("VerticalLayoutGroup Icon");

		#endregion

		#region Members

		private Button _back;
		private Button _forward;
		private ListView _listView;

		#endregion

		#region Window Management

		[MenuItem(WINDOW_MENU)]
		public static void Open()
		{
			var window = GetWindow<HistoryWindow>();
			window.titleContent = new GUIContent("History", _icon.Texture);
			window.Show();
		}

		[MenuItem(MOVE_BACK_MENU, validate = true)]
		public static bool CanMoveBack()
		{
			return HistoryList.Current > 0;
		}

		[MenuItem(MOVE_FORWARD_MENU, validate = true)]
		public static bool CanMoveForward()
		{
			return HistoryList.Current < HistoryList.History.Count - 1;
		}

		[MenuItem(MOVE_BACK_MENU, priority = 1)]
		public static void MoveBack()
		{
			if (CanMoveBack())
            {
                HistoryList.Select(HistoryList.Current - 1);
            }
        }

		[MenuItem(MOVE_FORWARD_MENU, priority = 2)]
		public static void MoveForward()
		{
			if (CanMoveForward())
            {
                HistoryList.Select(HistoryList.Current + 1);
            }
        }

		#endregion

		#region ListView Management

		private void OnEnable()
		{
			rootVisualElement.AddStyleSheet(STYLESHEET);
			rootVisualElement.AddToClassList(USS_CLASS_NAME);

			var header = new VisualElement();
			header.AddToClassList(HEADER_USS_CLASS_NAME);

			_back = new Button(MoveBack) { text = "Back" };
			_back.AddToClassList(HEADER_BUTTON_USS_CLASS_NAME);

			_forward = new Button(MoveForward) { text = "Forward" };
			_forward.AddToClassList(HEADER_BUTTON_USS_CLASS_NAME);

			_listView = new ListView(HistoryList.History, 21, MakeItem, BindItem);
			_listView.AddToClassList(LIST_USS_CLASS_NAME);
			_listView.selectionType = SelectionType.Single;
#if UNITY_2022_3_OR_NEWER
			_listView.itemsChosen += item => Select();
			_listView.selectionChanged += selection => Highlight();
#elif UNITY_2020_1_OR_NEWER
			_listView.onItemsChosen += item => Select();
			_listView.onSelectionChange += selection => Highlight();
#else
            _listView.onItemChosen += item => Select();
            _listView.onSelectionChanged += selection => Highlight();
#endif

			header.Add(_back);
			header.Add(_forward);
			rootVisualElement.Add(header);
			rootVisualElement.Add(_listView);

			Refresh();

			Selection.selectionChanged += Refresh;
		}

		private void OnDisable()
		{
			Selection.selectionChanged -= Refresh;
		}

		private void Refresh()
		{
			_back.SetEnabled(CanMoveBack());
			_forward.SetEnabled(CanMoveForward());
#if UNITY_2021_1_OR_NEWER
			_listView.Rebuild();
#else
			_listView.Refresh();
#endif
		}

		private VisualElement MakeItem()
		{
			return new HistoryItem();
		}

		private void BindItem(VisualElement item, int index)
		{
			if (item is HistoryItem history)
            {
                history.Bind(index);
            }
        }

		private void Select()
		{
			if (_listView.selectedIndex != HistoryList.Current)
			{
				HistoryList.Select(_listView.selectedIndex);
				Refresh();
			}
		}

		private void Highlight()
		{
			if (_listView.selectedItem is Object[] obj && obj.Length > 0)
            {
                EditorGUIUtility.PingObject(obj[0]);
            }
        }

		private class HistoryItem : VisualElement, IDraggable
		{
			public DragState DragState { get; set; }
			public string DragText => _label.text;
			public Object[] DragObjects => HistoryList.History[_index];
			public object DragData => DragObjects;

			private readonly Image _icon;
			private readonly Label _label;

			private int _index;

			public HistoryItem()
			{
				_icon = new Image();
				_icon.AddToClassList(LIST_ITEM_ICON_USS_CLASS_NAME);

				_label = new Label { pickingMode = PickingMode.Ignore };
				_label.AddToClassList(LIST_ITEM_LABEL_USS_CLASS_NAME);
				
				Add(_icon);
				Add(_label);
				AddToClassList(LIST_ITEM_USS_CLASS_NAME);

				this.MakeDraggable();
			}

			public void Bind(int index)
			{
				_index = index;
				_icon.image = HistoryList.GetIcon(_index);
				_label.text = HistoryList.GetName(_index);
				_label.EnableInClassList(CURRENT_LIST_ITEM_USS_CLASS_NAME, _index == HistoryList.Current);
			}
		}

		#endregion

		#region HistoryList Management

		private static class HistoryList
		{
			private const int CAPACITY = 100;

			private static bool _skipNextSelection = false;

			public static int Current { get; private set; }
			public static List<Object[]> History { get; private set; } = new List<Object[]>();

			static HistoryList()
			{
				Selection.selectionChanged += SelectionChanged;
				EditorApplication.playModeStateChanged += OnPlayModeChanged;
			}

			public static void Select(int index)
			{
				Current = index;
				_skipNextSelection = true;
				Selection.objects = History[index];
			}

			public static Texture GetIcon(int index)
			{
				var objects = History[index];

				return GetIcon(objects[0]);
			}

			private static Texture GetIcon(Object obj)
			{
				return obj ? AssetPreview.GetMiniThumbnail(obj) : null;
			}

			public static string GetName(int index)
			{
				var objects = History[index];
				if (objects.Length > 1)
                {
                    return string.Join(", ", objects.Select(GetName));
                }

                return GetName(objects[0]);
			}

			private static string GetName(Object obj)
			{
				if (obj == null)
                {
                    return "(missing)";
                }

                if (string.IsNullOrEmpty(obj.name))
                {
                    return $"'{obj.GetType().Name}'";
                }

                return obj.name;
			}

			private static void Clear()
			{
				Current = 0;
				History.Clear();
			}

			private static void SelectionChanged()
			{
				if (!_skipNextSelection)
				{
					if (Selection.objects != null && Selection.objects.Length > 0 && Selection.objects[0] is Object obj)
					{
						if (History.Count == 0 || History[Current][0] != obj)
						{
							var trailing = History.Count - Current - 1;

							if (trailing > 0)
                            {
                                History.RemoveRange(Current + 1, trailing);
                            }

                            if (Current == CAPACITY)
                            {
                                History.RemoveAt(0);
                            }

                            History.Add(Selection.objects);
							Current = History.Count - 1;
						}
					}
				}
				else
				{
					_skipNextSelection = false;
				}
			}

			private static void OnPlayModeChanged(PlayModeStateChange state)
			{
				switch (state)
				{
					case PlayModeStateChange.ExitingEditMode: Clear(); break;
					case PlayModeStateChange.EnteredEditMode: Clear(); break;
				}
			}
		}

		#endregion
	}
}