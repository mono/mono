/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       DataListComponentEditor
 *
 * Author:      Gaurav Vaish
 * Maintainer:  mastergaurav AT users DOT sf DOT net
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI.Design;
using System.Windows.Forms.Design;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design.WebControls
{
	public class DataListComponentEditor : BaseDataListComponentEditor
	{
		internal static int GENERAL = 0x00;
		internal static int FORMAT  = 0x01;
		internal static int BORDERS = 0x02;

		private static Type[] componentEditorPages;

		static DataListComponentEditor()
		{
			componentEditorPages = new Type[] {
				typeof(GeneralPageDataListInternal),
				typeof(FormatPageInternal),
				typeof(BordersPageInternal)
			};
		}

		public DataListComponentEditor() : base(GENERAL)
		{
		}
		
		public DataListComponentEditor(int initialPage) : base(initialPage)
		{
		}
		
		protected override Type[] GetComponentEditorPages()
		{
			return componentEditorPages;
		}
	}
}
