﻿using System.Collections.Generic;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	public class Tabs : VisualElement
	{
		#region Class Names

		public const string STYLESHEET = "Tabs.uss";
		public const string USS_CLASS_NAME = "pirho-tabs";
		public const string HEADER_USS_CLASS_NAME = USS_CLASS_NAME + "__header";
		public const string CONTENT_USS_CLASS_NAME = USS_CLASS_NAME + "__content";
		public const string TAB_USS_CLASS_NAME = USS_CLASS_NAME + "__tab";
		public const string TAB_SELECTED_USS_CLASS_NAME = TAB_USS_CLASS_NAME + "--selected";
		public const string PAGE_USS_CLASS_NAME = USS_CLASS_NAME + "__page";
		public const string PAGE_SELECTED_USS_CLASS_NAME = PAGE_USS_CLASS_NAME + "--selected";

		#endregion

		#region Members

		private TabPage _activePage;

		#endregion

		#region Public Interface

		public VisualElement Header { get; }
		public VisualElement Content { get; }
		public UQueryState<TabPage> Pages { get; }
		public TabPage ActivePage => _activePage;

		public override VisualElement contentContainer => Content;

		public Tabs()
		{
			Header = new VisualElement();
			Header.AddToClassList(HEADER_USS_CLASS_NAME);

			Content = new VisualElement();
			Content.AddToClassList(CONTENT_USS_CLASS_NAME);

			Pages = Content.Query<TabPage>().Build();

			hierarchy.Add(Header);
			hierarchy.Add(Content);

			AddToClassList(USS_CLASS_NAME);
			this.AddStyleSheet(STYLESHEET);
            
#if UNITY_2021_1_OR_NEWER
            this.style.marginRight = -2;
#else
            this.style.marginRight = 3;
#endif
		}

		public TabPage GetPage(string name)
		{
			TabPage found = null;

			Pages.ForEach(page =>
			{
				if (page.Label == name)
                {
                    found = page;
                }
            });

			return found;
		}

		public void UpdateTabs()
		{
			var pages = Pages;

			_activePage = null;
			Header.Clear();

			Pages.ForEach(page =>
			{
				Header.Add(page.Button);

				if (page.IsActive && _activePage == null)
                {
                    _activePage = page;
                }
            });

			if (_activePage == null && Content.childCount > 0)
            {
                _activePage = Content[0] as TabPage;
            }

            Pages.ForEach(page =>
			{
				page.IsActive = page == _activePage;
				page.EnableInClassList(PAGE_SELECTED_USS_CLASS_NAME, page.IsActive);
				page.Button.EnableInClassList(TAB_SELECTED_USS_CLASS_NAME, page.IsActive);
			});
		}

		#endregion

		#region UXML

		public new class UxmlFactory : UxmlFactory<Tabs, UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get
				{
					yield return new UxmlChildElementDescription(typeof(TabPage));
				}
			}
		}

		#endregion
	}
}
