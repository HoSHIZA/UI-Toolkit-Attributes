using UnityEngine;
using UnityEngine.UIElements;

#if !UNITY_2023_2_OR_NEWER
using UnityEditor.UIElements;
#endif

namespace PiRhoSoft.Utilities.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
#endif
	public partial class EulerField : BaseField<Quaternion>
	{
		#region Class Names

		public const string STYLESHEET = "EulerStyle.uss";
		public const string USS_CLASS_NAME = "pirho-euler-field";
		public const string LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label";
		public const string INPUT_USS_CLASS_NAME = USS_CLASS_NAME + "__input";

		#endregion

		#region Members

		private readonly Vector3Field _vectorField;

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("euler-angles")]
        private Vector3 Euler
        {
            get => _vectorField.value;
            set => _vectorField.SetValueWithoutNotify(value);
        }
#endif

		#endregion

		#region Public Interface

		public EulerField() : this(null)
		{
		}

		public EulerField(string label) : base(label, null)
		{
			_vectorField = new Vector3Field();
			_vectorField.AddToClassList(INPUT_USS_CLASS_NAME);
			_vectorField.RegisterCallback<ChangeEvent<Vector3>>(evt =>
			{
				base.value = Quaternion.Euler(evt.newValue);
				evt.StopImmediatePropagation();
			});

			labelElement.AddToClassList(LABEL_USS_CLASS_NAME);

			AddToClassList(USS_CLASS_NAME);
			this.SetVisualInput(_vectorField);
			this.AddStyleSheet(STYLESHEET);
		}

		public override void SetValueWithoutNotify(Quaternion newValue)
		{
			base.SetValueWithoutNotify(newValue);
			_vectorField.SetValueWithoutNotify(newValue.eulerAngles);
		}

		#endregion

		#region UXML Support

#if !UNITY_2023_2_OR_NEWER
		public new class UxmlFactory : UxmlFactory<EulerField, UxmlTraits> { }
		public new class UxmlTraits : BaseField<Quaternion>.UxmlTraits
		{
			private readonly UxmlFloatAttributeDescription _x = new UxmlFloatAttributeDescription { name = "x", defaultValue = 0.0f };
			private readonly UxmlFloatAttributeDescription _y = new UxmlFloatAttributeDescription { name = "y", defaultValue = 0.0f };
			private readonly UxmlFloatAttributeDescription _z = new UxmlFloatAttributeDescription { name = "z", defaultValue = 0.0f };

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);

				var euler = ve as EulerField;
				var x = _x.GetValueFromBag(bag, cc);
				var y = _y.GetValueFromBag(bag, cc);
				var z = _z.GetValueFromBag(bag, cc);

				euler.SetValueWithoutNotify(Quaternion.Euler(x, y, z));
			}
		}
#endif

		#endregion
	}
}
