using System.Reflection;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	public class Placeholder : Label
	{
		#region Class Names

		public const string STYLESHEET = "PlaceholderStyle.uss";
		public const string USS_CLASS_NAME = "pirho-placeholder";

		#endregion

		#region Cached Reflection Info

		private static readonly PropertyInfo _textInputProperty;
		private static readonly PropertyInfo _textProperty;

		static Placeholder()
		{
			_textInputProperty = typeof(TextField).GetProperty("textInput", BindingFlags.NonPublic | BindingFlags.Instance);
			_textProperty = _textInputProperty?.PropertyType?.GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
		}

		#endregion

		#region Public Interface

		public Placeholder() : this(null)
		{
		}

		public Placeholder(string text) : base(text)
		{
			pickingMode = PickingMode.Ignore;

			AddToClassList(USS_CLASS_NAME);
			this.AddStyleSheet(STYLESHEET);

			RegisterCallback<AttachToPanelEvent>(OnAttached);
		}

		public void AddToField(TextField textField)
		{
			// Add this specifically to the input field in case the TextField has a label
			var input = textField.Q(className: TextField.inputUssClassName);
			input.Add(this);

			UpdateDisplayed(textField);

#if UNITY_2023_1_OR_NEWER
			textField.RegisterCallback<InputEvent>(evt => UpdateDisplayed(textField));
#else
			textField.RegisterCallback<KeyDownEvent>(evt => UpdateDisplayed(textField));
#endif
		}

		#endregion

		#region State Management

		private void UpdateDisplayed(TextField field)
		{
			// Execute a frame later because text won't be updated yet
			schedule.Execute(() =>
			{
				var textInput = _textInputProperty.GetValue(field);
				var textString = _textProperty.GetValue(textInput) as string;

				this.SetDisplayed(string.IsNullOrEmpty(textString));
			}).StartingIn(0);
		}

		private void OnAttached(AttachToPanelEvent evt)
		{
			if (parent is TextField textField)
            {
                AddToField(textField);
            }
        }

		#endregion

		#region Events

#if UNITY_2023_2_OR_NEWER
		protected override void HandleEventBubbleUp(EventBase evt)
		{
			base.HandleEventBubbleUp(evt);
#else
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
#endif
			// Capture ChangeEvents so they aren't handled by the parent TextField.

			if (evt is ChangeEvent<string>)
			{
				evt.StopPropagation();
#if UNITY_2023_2_OR_NEWER
                focusController.IgnoreEvent(evt);
#else
                evt.PreventDefault();
#endif
			}
			else
			{
#if UNITY_2023_2_OR_NEWER
			base.HandleEventBubbleUp(evt);
#else
            base.ExecuteDefaultActionAtTarget(evt);
#endif
			}
		}

		#endregion

		#region UXML Support

		public new class UxmlFactory : UxmlFactory<Placeholder, UxmlTraits> { }

		#endregion
	}
}
