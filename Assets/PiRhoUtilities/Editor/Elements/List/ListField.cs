using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement("List")]
#endif
	public partial class ListField : Frame
	{
		#region Events

		public class ItemAddedEvent : EventBase<ItemAddedEvent>
		{
			public static ItemAddedEvent GetPooled(int index)
			{
				var e = GetPooled();
				e.Index = index;
				return e;
			}

			public int Index { get; private set; }

			public ItemAddedEvent()
			{
				LocalInit();
			}

			protected override void Init()
			{
				base.Init();
				LocalInit();
			}

            private void LocalInit()
			{
				Index = 0;
			}
		}

		public class ItemRemovedEvent : EventBase<ItemRemovedEvent>
		{
			public static ItemRemovedEvent GetPooled(int index)
			{
				var e = GetPooled();
				e.Index = index;
				return e;
			}

			public int Index { get; private set; }

			public ItemRemovedEvent()
			{
				LocalInit();
			}

			protected override void Init()
			{
				base.Init();
				LocalInit();
			}

            private void LocalInit()
			{
				Index = 0;
			}
		}

		public class ItemReorderedEvent : EventBase<ItemReorderedEvent>
		{
			public static ItemReorderedEvent GetPooled(int from, int to)
			{
				var e = GetPooled();
				e.FromIndex = from;
				e.ToIndex = to;
				return e;
			}

			public int FromIndex { get; private set; }
			public int ToIndex { get; private set; }

			public ItemReorderedEvent()
			{
				LocalInit();
			}

			protected override void Init()
			{
				base.Init();
				LocalInit();
			}

            private void LocalInit()
			{
				FromIndex = 0;
				ToIndex = 0;
			}
		}

		public class ItemsChangedEvent : EventBase<ItemsChangedEvent>
		{
		}

		#endregion

		#region Log Messages

		private const string INVALID_BINDING_ERROR = "(PUELFIB) invalid binding '{0}' for ListField: property '{1}' is type '{2}' but should be an array";
		private const string INVALID_TYPE_ERROR = "(PUELFIT) invalid item type '{0}' for ListField: the item type must be a default constructable class when used with allowDerived = false";
		private const string FAILED_ADD_ERROR = "(PUELFFA) failed to add item of type '{0}' to the ListField: the item type must be a value type or default constructable class that is compatible with the list";
		private const string UNSPECIFIED_TYPE = "(unspecified)";

		#endregion

		#region Class Names

		public new const string STYLESHEET = "ListStyle.uss";
		public new const string USS_CLASS_NAME = "pirho-list-field";
		public const string EMPTY_USS_CLASS_NAME = USS_CLASS_NAME + "--empty";
		public const string ADD_DISABLED_USS_CLASS_NAME = USS_CLASS_NAME + "--add-disabled";
		public const string REMOVE_DISABLED_USS_CLASS_NAME = USS_CLASS_NAME + "--remove-disabled";
		public const string REORDER_DISABLED_USS_CLASS_NAME = USS_CLASS_NAME + "--reorder-disabled";
		public const string EMPTY_LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__empty-label";
		public const string ITEMS_USS_CLASS_NAME = USS_CLASS_NAME + "__items";
		public const string ADD_BUTTON_USS_CLASS_NAME = USS_CLASS_NAME + "__add-button";
		public const string REMOVE_BUTTON_USS_CLASS_NAME = USS_CLASS_NAME + "__remove-button";
		public const string DRAG_HANDLE_USS_CLASS_NAME = USS_CLASS_NAME + "__drag-handle";
		public const string DRAG_PLACEHOLDER_USS_CLASS_NAME = USS_CLASS_NAME + "__drag-placeholder";
		public const string ITEM_USS_CLASS_NAME = USS_CLASS_NAME + "__item";
		public const string ITEM_DRAGGING_USS_CLASS_NAME = ITEM_USS_CLASS_NAME + "--dragging";
		public const string ITEM_EVEN_USS_CLASS_NAME = ITEM_USS_CLASS_NAME + "--even";
		public const string ITEM_ODD_USS_CLASS_NAME = ITEM_USS_CLASS_NAME + "--odd";
		public const string ITEM_CONTENT_USS_CLASS_NAME = ITEM_USS_CLASS_NAME + "__content";

		#endregion

		#region Defaults

		public const string DEFAULT_EMPTY_LABEL = "The list is empty";
		public const string DEFAULT_EMPTY_TOOLTIP = "There are no items in this list";
		public const string DEFAULT_ADD_TOOLTIP = "Add an item to this list";
		public const string DEFAULT_REMOVE_TOOLTIP = "Remove this item from the list";
		public const string DEFAULT_REORDER_TOOLTIP = "Move this item within the list";

		public const bool DEFAULT_ALLOW_ADD = true;
		public const bool DEFAULT_ALLOW_REMOVE = true;
		public const bool DEFAULT_ALLOW_REORDER = true;

		#endregion

		#region Icons

		private static readonly Icon _addIcon = Icon.Add;
		private static readonly Icon _removeIcon = Icon.Remove;
		private static readonly Icon _dragIcon = Icon.BuiltIn("animationnocurve");

		#endregion

		#region Members

		private string _emptyLabel = DEFAULT_EMPTY_LABEL;
		private string _emptyTooltip = DEFAULT_EMPTY_TOOLTIP;
		private string _addTooltip = DEFAULT_ADD_TOOLTIP;
		private string _removeTooltip = DEFAULT_REMOVE_TOOLTIP;
		private string _reorderTooltip = DEFAULT_REORDER_TOOLTIP;

		private bool _allowAdd = DEFAULT_ALLOW_ADD;
		private bool _allowRemove = DEFAULT_ALLOW_REMOVE;
		private bool _allowReorder = DEFAULT_ALLOW_REORDER;

		private IListProxy _proxy;

		private class TypeProvider : PickerProvider<Type> { }
		private TypeProvider _typeProvider;
		private Type _itemType;
		private bool _allowDerived = false;

		private IconButton _addButton;
		private UQueryState<IconButton> _removeButtons;
		private UQueryState<Image> _reorderHandles;
		private TextElement _emptyText;
		private VisualElement _itemsContainer;

		private int _dragFromIndex = -1;
		private int _dragToIndex = -1;
		private VisualElement _dragElement;
		private VisualElement _dragPlaceholder;

		#endregion

		#region Public Interface

		public ListField() : base(false)
		{
			BuildUi();
		}

#if UNITY_2023_2_OR_NEWER
        [Header("List")]
        [UxmlAttribute("empty-label")]
#endif
        public string EmptyLabel
        {
            get => _emptyLabel;
            set { _emptyLabel = value; UpdateEmptyLabel(); }
        }

#if UNITY_2023_2_OR_NEWER
        [Header("Tooltips")]
        [UxmlAttribute("empty-tooltip")]
#endif
        public string EmptyTooltip
        {
            get => _emptyTooltip;
            set { _emptyTooltip = value; UpdateEmptyLabel(); }
        }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("add-tooltip")]
#endif
        public string AddTooltip
        {
            get => _addTooltip;
            set { _addTooltip = value; UpdateAddLabel(); }
        }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("remove-tooltip")]
#endif
        public string RemoveTooltip
        {
            get => _removeTooltip;
            set { _removeTooltip = value; UpdateRemoveLabels(); }
        }

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("reorder-tooltip")]
#endif
        public string ReorderTooltip
        {
            get => _reorderTooltip;
            set { _reorderTooltip = value; UpdateReorderLabels(); }
        }

#if UNITY_2023_2_OR_NEWER
        [Header("Controls")]
        [UxmlAttribute("allow-add")]
#endif
        public bool AllowAdd
		{
			get => _allowAdd;
			set { _allowAdd = value; UpdateAddState(); }
		}

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("allow-remove")]
#endif
        public bool AllowRemove
		{
			get => _allowRemove;
			set { _allowRemove = value; UpdateRemoveState(); }
		}

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("allow-reorder")]
#endif
        public bool AllowReorder
		{
			get => _allowReorder;
			set { _allowReorder = value; UpdateReorderState(); }
		}

		public IListProxy Proxy => _proxy;
		public Type ItemType => _itemType;
		public bool AllowDerived => _allowDerived;

		public void SetProxy(IListProxy proxy, Type itemType, bool allowDerived)
		{
			if (itemType != null && !allowDerived && !itemType.IsCreatable())
			{
				Debug.LogWarningFormat(INVALID_TYPE_ERROR, itemType.FullName);
				return;
			}

			_proxy = proxy;
			_itemType = itemType;
			_allowDerived = allowDerived && itemType != null;

			UpdateProxy();
			UpdateItemType();
		}

		public void Rebuild()
		{
			UpdateItemsWithoutNotify();
		}

		#endregion

		#region Ui

		private void BuildUi()
		{
			AddToClassList(USS_CLASS_NAME);
			this.AddStyleSheet(STYLESHEET);

			_addButton = AddHeaderButton(_addIcon.Texture, _addTooltip, ADD_BUTTON_USS_CLASS_NAME, DoAdd);
			_removeButtons = Content.Query<IconButton>(className: REMOVE_BUTTON_USS_CLASS_NAME).Build();
			_reorderHandles = Content.Query<Image>(className: DRAG_HANDLE_USS_CLASS_NAME).Build();

			_emptyText = new TextElement();
			_emptyText.AddToClassList(EMPTY_LABEL_USS_CLASS_NAME);

			_itemsContainer = new VisualElement();
			_itemsContainer.AddToClassList(ITEMS_USS_CLASS_NAME);

			Content.Add(_emptyText);
			Content.Add(_itemsContainer);

			_dragPlaceholder = new VisualElement();
			_dragPlaceholder.AddToClassList(DRAG_PLACEHOLDER_USS_CLASS_NAME);

			_typeProvider = ScriptableObject.CreateInstance<TypeProvider>();

			UpdateAddState();
			UpdateRemoveState();
			UpdateReorderState();

			UpdateEmptyLabel();
			UpdateAddLabel();
			UpdateRemoveLabels();
			UpdateReorderLabels();

			UpdateProxy();
			UpdateItemType();
		}

		private void UpdateAddState()
		{
			EnableInClassList(ADD_DISABLED_USS_CLASS_NAME, !_allowAdd);
		}

		private void UpdateRemoveState()
		{
			EnableInClassList(REMOVE_DISABLED_USS_CLASS_NAME, !_allowRemove);
		}

		private void UpdateReorderState()
		{
			EnableInClassList(REORDER_DISABLED_USS_CLASS_NAME, !_allowReorder);

			UnregisterCallback<MouseMoveEvent>(UpdateDrag);
			UnregisterCallback<MouseUpEvent>(StopDrag);

			if (_allowReorder)
			{
				RegisterCallback<MouseMoveEvent>(UpdateDrag);
				RegisterCallback<MouseUpEvent>(StopDrag);
			}
		}

		private void UpdateEmptyState()
		{
			EnableInClassList(EMPTY_USS_CLASS_NAME, _proxy.Count == 0);
		}

		private void UpdateEmptyLabel()
		{
			_emptyText.text = _emptyLabel;
			_emptyText.tooltip = _emptyTooltip;
		}

		private void UpdateAddLabel()
		{
			_addButton.tooltip = _addTooltip;
		}

		private void UpdateRemoveLabels()
		{
			_removeButtons.ForEach(button => button.tooltip = _removeTooltip);
		}

		private void UpdateReorderLabels()
		{
			_reorderHandles.ForEach(button => button.tooltip = _reorderTooltip);
		}

		private void UpdateProxy()
		{
			_itemsContainer.Clear();

			if (_proxy != null)
            {
                UpdateItemsWithoutNotify();
            }
            else
            {
                EnableInClassList(EMPTY_USS_CLASS_NAME, true);
            }
        }

		private void UpdateItemType()
		{
			if (_itemType != null)
			{
				var types = TypeHelper.GetTypeList(_itemType, false);
				_typeProvider.Setup(_itemType.Name, types.Paths, types.Types, GetIcon, AddItem);
			}
		}

		private void UpdateItems()
		{
			UpdateItemsWithoutNotify();

			using (var e = ItemsChangedEvent.GetPooled())
			{
				e.target = this;
				SendEvent(e);
			}
		}

		private void UpdateItemsWithoutNotify()
		{
			UpdateEmptyState();

			while (_itemsContainer.childCount > _proxy.Count)
				_itemsContainer.RemoveAt(_itemsContainer.childCount - 1);

			for (var i = 0; i < _proxy.Count; i++)
			{
				if (i < _itemsContainer.childCount)
                {
                    CheckElement(i);
                }
                else
                {
                    CreateElement(i);
                }
            }

			_removeButtons.ForEach(button =>
			{
				var index = GetIndex(button.parent);
				var removable = _proxy.CanRemove(index);

				button.SetEnabled(removable);
			});

			var validAdd = _proxy.CanAdd();
			var validType = _allowDerived || _proxy.CanAdd(_itemType);

			_addButton.SetEnabled(validAdd && validType);
		}

		private void CreateElement(int index)
		{
			var item = new VisualElement();
			item.AddToClassList(ITEM_USS_CLASS_NAME);
			_itemsContainer.Add(item);

			var dragHandle = new Image { image = _dragIcon.Texture, tooltip = _reorderTooltip };
			dragHandle.AddToClassList(DRAG_HANDLE_USS_CLASS_NAME);
			dragHandle.RegisterCallback((MouseDownEvent e) => StartDrag(e, item));
			item.Add(dragHandle);

			var remove = new IconButton(() => RemoveItem(item)) { image = _removeIcon.Texture, tooltip = _removeTooltip };
			remove.AddToClassList(REMOVE_BUTTON_USS_CLASS_NAME);
			item.Add(remove);

			UpdateContent(item, index);
		}

		private void CheckElement(int index)
		{
			// TODO: tracking by index doesn't work since indices change (unlike keys in a dictionary) - need some other way to associate elements with items

			var item = _itemsContainer[index];
			//var current = GetKey(item);

			//if (index != current)
			{
				item.RemoveAt(1);
				UpdateContent(item, index);
			}
		}

		private void UpdateContent(VisualElement item, int index)
		{
			item.EnableInClassList(ITEM_EVEN_USS_CLASS_NAME, index % 2 == 0);
			item.EnableInClassList(ITEM_ODD_USS_CLASS_NAME, index % 2 != 0);

			var content = _proxy.CreateElement(index);
			content.AddToClassList(ITEM_CONTENT_USS_CLASS_NAME);
			item.Insert(1, content);
		}

		#endregion

		#region Item Management

		private int GetIndex(VisualElement element)
		{
			return element.parent.IndexOf(element);
		}

		private void DoAdd()
		{
			if (_allowDerived)
            {
                SelectType();
            }
            else
            {
                AddItem(_itemType);
            }
        }

		private void SelectType()
		{
			if (_allowAdd && _proxy.CanAdd())
			{
				var position = new Vector2(_addButton.worldBound.center.x, _addButton.worldBound.yMax + _addButton.worldBound.height * 0.5f);
				SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(position)), _typeProvider);
			}
		}

		private Texture GetIcon(Type type)
		{
			return AssetPreview.GetMiniTypeThumbnail(type);
		}

		private void AddItem(Type selected)
		{
			if (_allowAdd && _proxy.CanAdd() && _proxy.CanAdd(selected))
			{
				if (_proxy.AddItem(selected))
				{
					CreateElement(_proxy.Count - 1);
					UpdateEmptyState();

					using (var e = ItemAddedEvent.GetPooled(_proxy.Count - 1))
					{
						e.target = this;
						SendEvent(e);
					}
				}
				else
				{
					Debug.LogErrorFormat(FAILED_ADD_ERROR, selected != null ? selected.FullName : UNSPECIFIED_TYPE);
				}
			}
		}

		private void RemoveItem(VisualElement item)
		{
			var index = GetIndex(item);

			if (_allowRemove && _proxy.CanRemove(index))
			{
				item.RemoveFromHierarchy();
				Proxy.RemoveItem(index);
				UpdateItemsWithoutNotify();

				using (var e = ItemRemovedEvent.GetPooled(index))
				{
					e.target = this;
					SendEvent(e);
				}
			}
		}

		private void ReorderItem(int from, int to)
		{
			Proxy.ReorderItem(from, to);
			UpdateItemsWithoutNotify();

			using (var e = ItemReorderedEvent.GetPooled(from, to))
			{
				e.target = this;
				SendEvent(e);
			}
		}

		#endregion

		#region Dragging

		private void StartDrag(MouseDownEvent e, VisualElement item)
		{
			if (_allowReorder && e.button == (int)MouseButton.LeftMouse)
			{
				var index = GetIndex(item);
				var mousePosition = _itemsContainer.WorldToLocal(e.mousePosition);

				_dragFromIndex = index;
				_dragToIndex = index;

				_dragElement = _itemsContainer.ElementAt(index);
				_dragElement.AddToClassList(ITEM_DRAGGING_USS_CLASS_NAME);
				_dragElement.BringToFront();
				_dragElement.style.left = mousePosition.x;
				_dragElement.style.top = mousePosition.y;

				_itemsContainer.Insert(index, _dragPlaceholder);

				this.CaptureMouse();
			}
		}

		private void UpdateDrag(MouseMoveEvent e)
		{
			if (e.button == (int)MouseButton.LeftMouse)
			{
				if (_dragElement != null)
				{
					var mousePosition = _itemsContainer.WorldToLocal(e.mousePosition);

					_dragElement.style.left = mousePosition.x;
					_dragElement.style.top = mousePosition.y;

					var nextIndex = -1;
					VisualElement nextElement = null;

					for (var i = 0; i < _itemsContainer.childCount - 1; i++)
					{
						if (mousePosition.y < _itemsContainer.ElementAt(i).localBound.center.y)
						{
							nextIndex = i;
							nextElement = _itemsContainer.ElementAt(i);
							break;
						}
					}

					if (nextElement != null)
					{
						_dragToIndex = nextIndex > _dragToIndex ? nextIndex - 1 : nextIndex;
						_dragPlaceholder.PlaceBehind(nextElement);
					}
					else
					{
						_dragToIndex = _itemsContainer.childCount - 2; // Subtract 2 because _dragPlaceholder counts as a child
						_dragPlaceholder.PlaceBehind(_dragElement);
					}
				}
			}
		}

		private void StopDrag(MouseUpEvent e)
		{
			if (e.button == (int)MouseButton.LeftMouse)
			{
				this.ReleaseMouse();

				if (_dragElement != null)
				{
					_dragElement.style.left = 0;
					_dragElement.style.top = 0;
					_dragElement.PlaceBehind(_dragPlaceholder);
					_dragElement.RemoveFromClassList(ITEM_DRAGGING_USS_CLASS_NAME);
				}

				_dragPlaceholder.RemoveFromHierarchy();

				if (_dragFromIndex != _dragToIndex)
                {
                    ReorderItem(_dragFromIndex, _dragToIndex);
                }

                _dragElement = null;
				_dragFromIndex = -1;
				_dragToIndex = -1;
			}
		}

		#endregion

		#region Binding

#if UNITY_2023_2_OR_NEWER
		protected override void HandleEventBubbleUp(EventBase evt)
		{
			base.HandleEventBubbleUp(evt);
#else
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
#endif

			if (this.TryGetPropertyBindEvent(evt, out var property))
			{
				var arrayProperty = property.FindPropertyRelative("Array.size");

				if (arrayProperty != null)
				{
					var sizeBinding = new ChangeTrigger<int>(null, (_, oldSize, newSize) => UpdateItems());
					sizeBinding.Watch(arrayProperty);

					Add(sizeBinding);
				}
				else
				{
					Debug.LogErrorFormat(INVALID_BINDING_ERROR, bindingPath, property.propertyPath, property.propertyType);
				}
			}
		}

		#endregion

		#region UXML Support

#if !UNITY_2023_2_OR_NEWER
		public new class UxmlFactory : UxmlFactory<ListField, UxmlTraits> { }
		public new class UxmlTraits : Frame.UxmlTraits
		{
			private readonly UxmlBoolAttributeDescription _allowAdd = new UxmlBoolAttributeDescription { name = "allow-add", defaultValue = DEFAULT_ALLOW_ADD };
			private readonly UxmlBoolAttributeDescription _allowRemove = new UxmlBoolAttributeDescription { name = "allow-remove", defaultValue = DEFAULT_ALLOW_REMOVE };
			private readonly UxmlBoolAttributeDescription _allowReorder = new UxmlBoolAttributeDescription { name = "allow-reorder", defaultValue = DEFAULT_ALLOW_REORDER };
			private readonly UxmlStringAttributeDescription _emptyLabel = new UxmlStringAttributeDescription { name = "empty-label", defaultValue = DEFAULT_EMPTY_LABEL };
			private readonly UxmlStringAttributeDescription _emptyTooltip = new UxmlStringAttributeDescription { name = "empty-tooltip", defaultValue = DEFAULT_EMPTY_TOOLTIP };
			private readonly UxmlStringAttributeDescription _addTooltip = new UxmlStringAttributeDescription { name = "add-tooltip", defaultValue = DEFAULT_ADD_TOOLTIP };
			private readonly UxmlStringAttributeDescription _removeTooltip = new UxmlStringAttributeDescription { name = "remove-tooltip", defaultValue = DEFAULT_REMOVE_TOOLTIP };
			private readonly UxmlStringAttributeDescription _reorderTooltip = new UxmlStringAttributeDescription { name = "reorder-tooltip", defaultValue = DEFAULT_REORDER_TOOLTIP };

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);

				var list = (ListField) ve;

				list.AllowAdd = _allowAdd.GetValueFromBag(bag, cc);
				list.AllowRemove = _allowRemove.GetValueFromBag(bag, cc);
				list.AllowReorder = _allowReorder.GetValueFromBag(bag, cc);
				list.EmptyLabel = _emptyLabel.GetValueFromBag(bag, cc);
				list.EmptyTooltip = _emptyTooltip.GetValueFromBag(bag, cc);
				list.AddTooltip =_addTooltip.GetValueFromBag(bag, cc);
				list.RemoveTooltip = _removeTooltip.GetValueFromBag(bag, cc);
				list.ReorderTooltip = _reorderTooltip.GetValueFromBag(bag, cc);
			}
		}
#endif

		#endregion
	}
}
