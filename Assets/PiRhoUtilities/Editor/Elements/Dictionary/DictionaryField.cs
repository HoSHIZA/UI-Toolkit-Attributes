using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement("Dictionary")]
#endif
	public partial class DictionaryField : Frame
	{
		#region Events

		public class ItemAddedEvent : EventBase<ItemAddedEvent>
		{
			public static ItemAddedEvent GetPooled(string key)
			{
				var e = GetPooled();
				e.Key = key;
				return e;
			}

			public string Key { get; private set; }

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
				Key = string.Empty;
			}
		}

		public class ItemRemovedEvent : EventBase<ItemRemovedEvent>
		{
			public static ItemRemovedEvent GetPooled(string key)
			{
				var e = GetPooled();
				e.Key = key;
				return e;
			}

			public string Key { get; private set; }

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
				Key = string.Empty;
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

		private const string INVALID_BINDING_ERROR = "(PUEDFIB) invalid binding '{0}' for DictionaryField: property '{1}' is type '{2}' but should be an array";
		private const string INVALID_TYPE_ERROR = "(PUEDFIT) invalid item type '{0}' for DictionaryField: the item type must be a default constructable class when used with allowDerived = false";
		private const string FAILED_ADD_ERROR = "(PUEDFFA) failed to add item '{0}' of type '{1}' to the DictionaryField: the item type must be a value type or default constructable class that is compatible with the dictionary";
		private const string UNSPECIFIED_TYPE = "(unspecified)";

		#endregion

		#region Class Names

		public new const string STYLESHEET = "DictionaryStyle.uss";
		public new const string USS_CLASS_NAME = "pirho-dictionary-field";
		public const string EMPTY_USS_CLASS_NAME = USS_CLASS_NAME + "--empty";
		public const string ADD_DISABLED_USS_CLASS_NAME = USS_CLASS_NAME + "--add-disabled";
		public const string REMOVE_DISABLED_USS_CLASS_NAME = USS_CLASS_NAME + "--remove-disabled";
		public const string REORDER_DISABLED_USS_CLASS_NAME = USS_CLASS_NAME + "--reorder-disabled";
		public const string ADD_KEY_VALID_USS_CLASS_NAME = USS_CLASS_NAME + "--add-key-valid";
		public const string ADD_KEY_INVALID_USS_CLASS_NAME = USS_CLASS_NAME + "--add-key-invalid";
		public const string EMPTY_LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__empty-label";
		public const string ITEMS_USS_CLASS_NAME = USS_CLASS_NAME + "__items";
		public const string HEADER_KEY_TEXT_USS_CLASS_NAME = USS_CLASS_NAME + "__key-text";
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

		public const string DEFAULT_EMPTY_LABEL = "The dictionary is empty";
		public const string DEFAULT_EMPTY_TOOLTIP = "There are no items in this dictionary";
		public const string DEFAULT_ADD_PLACEHOLDER = "New key";
		public const string DEFAULT_ADD_TOOLTIP = "Add an item to this dictionary";
		public const string DEFAULT_REMOVE_TOOLTIP = "Remove this item from the dictionary";
		public const string DEFAULT_REORDER_TOOLTIP = "Move this item within the dictionary";

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

#if UNITY_2023_2_OR_NEWER
        [Header("Dictionary")]
        [UxmlAttribute("empty-label")]
#endif
		private string _emptyLabel = DEFAULT_EMPTY_LABEL;
        
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("add-placeholder")]
#endif
        private string _addPlaceholder = DEFAULT_ADD_PLACEHOLDER;
        
#if UNITY_2023_2_OR_NEWER
        [Header("Tooltips")]
        [UxmlAttribute("empty-tooltip")]
#endif
		private string _emptyTooltip = DEFAULT_EMPTY_TOOLTIP;
        
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("add-tooltip")]
#endif
		private string _addTooltip = DEFAULT_ADD_TOOLTIP;
        
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("remove-tooltip")]
#endif
        private string _removeTooltip = DEFAULT_REMOVE_TOOLTIP;
        
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("reorder-tooltip")]
#endif
        private string _reorderTooltip = DEFAULT_REORDER_TOOLTIP;

#if UNITY_2023_2_OR_NEWER
        [Header("Controls")]
        [UxmlAttribute("allow-add")]
#endif
		private bool _allowAdd = DEFAULT_ALLOW_ADD;
        
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("allow-remove")]
#endif
		private bool _allowRemove = DEFAULT_ALLOW_REMOVE;
        
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("allow-reorder")]
#endif
		private bool _allowReorder = DEFAULT_ALLOW_REORDER;

		private IDictionaryProxy _proxy;

		private class TypeProvider : PickerProvider<Type> { }
		private TypeProvider _typeProvider;
		private Type _itemType;
		private bool _allowDerived = false;

		private TextField _addField;
		private Placeholder _addPlaceholderText;
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

		public DictionaryField() : base(false)
		{
			BuildUi();
		}
        
		public bool AllowAdd
		{
			get => _allowAdd;
			set { _allowAdd = value; UpdateAddState(); }
		}

		public bool AllowRemove
		{
			get => _allowRemove;
			set { _allowRemove = value; UpdateRemoveState(); }
		}

		public bool AllowReorder
		{
			get => _allowReorder;
			set { _allowReorder = value; UpdateReorderState(); }
		}

		public string EmptyLabel
		{
			get => _emptyLabel;
			set { _emptyLabel = value; UpdateEmptyLabel(); }
		}

		public string EmptyTooltip
		{
			get => _emptyTooltip;
			set { _emptyTooltip = value; UpdateEmptyLabel(); }
		}

		public string AddTooltip
		{
			get => _addTooltip;
			set { _addTooltip = value; UpdateAddLabel(); }
		}

		public string AddPlaceholder
		{
			get => _addPlaceholder;
			set { _addPlaceholder = value; UpdateAddPlaceholder(); }
		}

		public string RemoveTooltip
		{
			get => _removeTooltip;
			set { _removeTooltip = value; UpdateRemoveLabels(); }
		}

		public string ReorderTooltip
		{
			get => _reorderTooltip;
			set { _reorderTooltip = value; UpdateReorderLabels(); }
		}

		public IDictionaryProxy Proxy => _proxy;
		public Type ItemType => _itemType;
		public bool AllowDerived => _allowDerived;

		public void SetProxy(IDictionaryProxy proxy, Type itemType, bool allowDerived)
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
			UpdateReorderState();
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
            
#if UNITY_2021_1_OR_NEWER
            this.style.marginRight = -2;
#else
            this.style.marginRight = 3;
#endif

			_addField = new TextField();
			_addField.AddToClassList(HEADER_KEY_TEXT_USS_CLASS_NAME);
			_addField.RegisterValueChangedCallback(evt => AddKeyChanged(evt.newValue));
			_addField.Q(TextField.textInputUssName).RegisterCallback<KeyDownEvent>(evt => AddKeyPressed(evt));

			_addPlaceholderText = new Placeholder(_addPlaceholder);
			_addPlaceholderText.AddToField(_addField);

			Header.Add(_addField);
			_addField.PlaceBehind(HeaderButtons);

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
			UpdateAddPlaceholder();
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
			var allow = _allowReorder && (_proxy != null && _proxy.IsReorderable);

			EnableInClassList(REORDER_DISABLED_USS_CLASS_NAME, !allow);

			UnregisterCallback<MouseMoveEvent>(UpdateDrag);
			UnregisterCallback<MouseUpEvent>(StopDrag);

			if (allow)
			{
				RegisterCallback<MouseMoveEvent>(UpdateDrag);
				RegisterCallback<MouseUpEvent>(StopDrag);
			}
		}

		private void UpdateEmptyState()
		{
			EnableInClassList(EMPTY_USS_CLASS_NAME, _proxy == null || _proxy.Count == 0);
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

		private void UpdateAddPlaceholder()
		{
			_addPlaceholderText.text = _addPlaceholder;
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
                Debug.Log("aboba");
				SendEvent(e);
			}
		}

		private void UpdateItemsWithoutNotify()
		{
			UpdateEmptyState();

            while (_itemsContainer.childCount > _proxy.Count)
            {
                _itemsContainer.RemoveAt(_itemsContainer.childCount - 1);
            }

			for (var i = 0; i < _proxy.Count; i++)
			{
				var key = _proxy.GetKey(i);

				if (i < _itemsContainer.childCount)
                {
                    CheckElement(i, key);
                }
                else
                {
                    CreateElement(i, key);
                }
            }

			AddKeyChanged(_addField.text);

			_removeButtons.ForEach(button =>
			{
				var key = GetKey(button.parent);
				var index = GetIndex(button.parent);
				var removable = _proxy.CanRemove(index, key);

				button.SetEnabled(removable);
			});
		}

		private void CreateElement(int index, string key)
		{
			var item = new VisualElement();
			item.AddToClassList(ITEM_USS_CLASS_NAME);
			_itemsContainer.Add(item);

            item.name = key;

			var dragHandle = new Image { image = _dragIcon.Texture, tooltip = _reorderTooltip };
			dragHandle.AddToClassList(DRAG_HANDLE_USS_CLASS_NAME);
			dragHandle.RegisterCallback((MouseDownEvent e) => StartDrag(e, item));
			item.Add(dragHandle);

			var remove = new IconButton(() => RemoveItem(item)) { image = _removeIcon.Texture, tooltip = _removeTooltip };
			remove.AddToClassList(REMOVE_BUTTON_USS_CLASS_NAME);
			item.Add(remove);

			UpdateContent(item, index, key);
		}

		private void CheckElement(int index, string key)
		{
			//  All items needs to be rebuilt because any bindings will sometimes become invalid

			var item = _itemsContainer[index];
			var current = GetKey(item);
            
			if (key != current)
			{
				item.RemoveAt(1);
				UpdateContent(item, index, key);
			}
		}

		private void UpdateContent(VisualElement item, int index, string key)
		{
			SetKey(item, key);
			item.EnableInClassList(ITEM_EVEN_USS_CLASS_NAME, index % 2 == 0);
			item.EnableInClassList(ITEM_ODD_USS_CLASS_NAME, index % 2 != 0);

			var content = _proxy.CreateElement(index, key);
            
			content.AddToClassList(ITEM_CONTENT_USS_CLASS_NAME);
			item.Insert(1, content);
		}

		#endregion

		#region Item Management

		private int GetIndex(VisualElement element)
		{
			return element.parent.IndexOf(element);
		}

		private string GetKey(VisualElement element)
		{
			return element.userData is string s ? s : null;
		}

		private void SetKey(VisualElement element, string key)
		{
			element.userData = key;
		}

		private void AddKeyChanged(string newValue)
		{
			// Separately check for empty so it shows up as valid even if it's not.

			var empty = string.IsNullOrEmpty(newValue);
			var validKey = _proxy != null && _proxy.CanAdd(newValue);
			var validType = _proxy != null && (_allowDerived || _proxy.CanAdd(_itemType));

			EnableInClassList(ADD_KEY_INVALID_USS_CLASS_NAME, !empty && !validKey);
			EnableInClassList(ADD_KEY_VALID_USS_CLASS_NAME, !empty && validKey);

			_addButton.SetEnabled(validKey && validType);
		}

		private void AddKeyPressed(KeyDownEvent evt)
		{
			if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return)
			{
				DoAdd();
                
				evt.StopPropagation();
#if UNITY_2023_2_OR_NEWER
                focusController.IgnoreEvent(evt);
#else
                evt.PreventDefault();
#endif
			}
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
			var key = _addField.text;

			if (_allowAdd && _proxy.CanAdd(key))
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
			var key = _addField.text;

			if (_allowAdd && _proxy.CanAdd(key) && _proxy.CanAdd(selected))
			{
				if (!_proxy.AddItem(key, selected))
				{
					Debug.LogErrorFormat(FAILED_ADD_ERROR, key, selected != null ? selected.FullName : UNSPECIFIED_TYPE);
					return;
				}

				_addField.value = string.Empty;
				UpdateItemsWithoutNotify();

				using (var e = ItemAddedEvent.GetPooled(key))
				{
					e.target = this;
					SendEvent(e);
				}
			}
		}

		private void RemoveItem(VisualElement item)
		{
			var key = GetKey(item);
			var index = GetIndex(item);

			if (_allowRemove && _proxy.CanRemove(index, key))
			{
				Proxy.RemoveItem(index, key);
				UpdateItemsWithoutNotify();

				using (var e = ItemRemovedEvent.GetPooled(key))
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
		public new class UxmlFactory : UxmlFactory<DictionaryField, UxmlTraits> { }
		public new class UxmlTraits : Frame.UxmlTraits
		{
			private readonly UxmlBoolAttributeDescription _allowAdd = new UxmlBoolAttributeDescription { name = "allow-add", defaultValue = DEFAULT_ALLOW_ADD };
			private readonly UxmlBoolAttributeDescription _allowRemove = new UxmlBoolAttributeDescription { name = "allow-remove", defaultValue = DEFAULT_ALLOW_REMOVE };
			private readonly UxmlBoolAttributeDescription _allowReorder = new UxmlBoolAttributeDescription { name = "allow-reorder", defaultValue = DEFAULT_ALLOW_REORDER };
			private readonly UxmlStringAttributeDescription _emptyLabel = new UxmlStringAttributeDescription { name = "empty-label", defaultValue = DEFAULT_EMPTY_LABEL };
			private readonly UxmlStringAttributeDescription _emptyTooltip = new UxmlStringAttributeDescription { name = "empty-tooltip", defaultValue = DEFAULT_EMPTY_TOOLTIP };
			private readonly UxmlStringAttributeDescription _addTooltip = new UxmlStringAttributeDescription { name = "add-tooltip", defaultValue = DEFAULT_ADD_TOOLTIP };
			private readonly UxmlStringAttributeDescription _addPlaceholder = new UxmlStringAttributeDescription { name = "add-placeholder", defaultValue = DEFAULT_ADD_PLACEHOLDER };
			private readonly UxmlStringAttributeDescription _removeTooltip = new UxmlStringAttributeDescription { name = "remove-tooltip", defaultValue = DEFAULT_REMOVE_TOOLTIP };
			private readonly UxmlStringAttributeDescription _reorderTooltip = new UxmlStringAttributeDescription { name = "reorder-tooltip", defaultValue = DEFAULT_REORDER_TOOLTIP };

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);

				var dictionary = (DictionaryField)ve;

				dictionary.AllowAdd = _allowAdd.GetValueFromBag(bag, cc);
				dictionary.AllowRemove = _allowRemove.GetValueFromBag(bag, cc);
				dictionary.AllowReorder = _allowReorder.GetValueFromBag(bag, cc);
				dictionary.EmptyLabel = _emptyLabel.GetValueFromBag(bag, cc);
				dictionary.EmptyTooltip = _emptyTooltip.GetValueFromBag(bag, cc);
				dictionary.AddTooltip = _addTooltip.GetValueFromBag(bag, cc);
				dictionary.AddPlaceholder = _addPlaceholder.GetValueFromBag(bag, cc);
				dictionary.RemoveTooltip = _removeTooltip.GetValueFromBag(bag, cc);
				dictionary.ReorderTooltip = _reorderTooltip.GetValueFromBag(bag, cc);
			}
		}
#endif

		#endregion
	}
}
