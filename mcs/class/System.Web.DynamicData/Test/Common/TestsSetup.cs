using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using MonoTests.SystemWeb.Framework;

namespace MonoTests.Common
{
	public static class TestsSetup
	{
		static readonly char[] buildPathSplitChars = { '/' };

		public static string BuildPath (string path)
		{
			if (String.IsNullOrEmpty (path))
				return String.Empty;

			StringBuilder ret = new StringBuilder ();
			bool first = true;
			foreach (string s in path.Split (buildPathSplitChars)) {
				if (!first)
					ret.Append (Path.DirectorySeparatorChar);
				else
					first = false;
				ret.Append (s);
			}

			return ret.ToString ();
		}

		public static void CopyResources ()
		{
			Type type = typeof (TestsSetup);
			WebTest.CopyResource (type, "MonoTests.WebPages.Global.asax", "Global.asax");
#if NET_4_5
			WebTest.CopyResource (type, "MonoTests.WebPages.web.config.4.5", "web.config");
#elif NET_4_0
			WebTest.CopyResource (type, "MonoTests.WebPages.web.config.4.0", "web.config");
#else
			WebTest.CopyResource (type, "MonoTests.WebPages.web.config.2.0", "web.config");
#endif
			WebTest.CopyResource (type, "MonoTests.WebPages.Site.css", "Site.css");
			WebTest.CopyResource (type, "MonoTests.WebPages.Site.master", "Site.master");
			WebTest.CopyResource (type, "MonoTests.WebPages.Site.master.cs", "Site.master.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.web.config", BuildPath ("DynamicData/web.config"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.FilterUserControl.ascx", BuildPath ("DynamicData/Content/FilterUserControl.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.FilterUserControl.ascx.cs", BuildPath ("DynamicData/Content/FilterUserControl.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.GridViewPager.ascx", BuildPath ("DynamicData/Content/GridViewPager.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.GridViewPager.ascx.cs", BuildPath ("DynamicData/Content/GridViewPager.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.Back.gif", BuildPath ("DynamicData/Content/Images/Back.gif"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.header_back.gif", BuildPath ("DynamicData/Content/Images/header_back.gif"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.PgFirst.gif", BuildPath ("DynamicData/Content/Images/PgFirst.gif"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.PgLast.gif", BuildPath ("DynamicData/Content/Images/PgLast.gif"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.PgNext.gif", BuildPath ("DynamicData/Content/Images/PgNext.gif"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.PgPrev.gif", BuildPath ("DynamicData/Content/Images/PgPrev.gif"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.plus.gif", BuildPath ("DynamicData/Content/Images/plus.gif"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Boolean.ascx", BuildPath ("DynamicData/FieldTemplates/Boolean.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Boolean.ascx.cs", BuildPath ("DynamicData/FieldTemplates/Boolean.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Boolean_Edit.ascx", BuildPath ("DynamicData/FieldTemplates/Boolean_Edit.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Boolean_Edit.ascx.cs", BuildPath ("DynamicData/FieldTemplates/Boolean_Edit.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Children.ascx", BuildPath ("DynamicData/FieldTemplates/Children.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Children.ascx.cs", BuildPath ("DynamicData/FieldTemplates/Children.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.DateTime.ascx", BuildPath ("DynamicData/FieldTemplates/DateTime.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.DateTime.ascx.cs", BuildPath ("DynamicData/FieldTemplates/DateTime.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.DateTime_Edit.ascx", BuildPath ("DynamicData/FieldTemplates/DateTime_Edit.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.DateTime_Edit.ascx.cs", BuildPath ("DynamicData/FieldTemplates/DateTime_Edit.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Decimal_Edit.ascx", BuildPath ("DynamicData/FieldTemplates/Decimal_Edit.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Decimal_Edit.ascx.cs", BuildPath ("DynamicData/FieldTemplates/Decimal_Edit.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey.ascx", BuildPath ("DynamicData/FieldTemplates/ForeignKey.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey.ascx.cs", BuildPath ("DynamicData/FieldTemplates/ForeignKey.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey_Edit.ascx", BuildPath ("DynamicData/FieldTemplates/ForeignKey_Edit.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey_Edit.ascx.cs", BuildPath ("DynamicData/FieldTemplates/ForeignKey_Edit.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Integer_Edit.ascx", BuildPath ("DynamicData/FieldTemplates/Integer_Edit.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Integer_Edit.ascx.cs", BuildPath ("DynamicData/FieldTemplates/Integer_Edit.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.MultilineText_Edit.ascx", BuildPath ("DynamicData/FieldTemplates/MultilineText_Edit.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.MultilineText_Edit.ascx.cs", BuildPath ("DynamicData/FieldTemplates/MultilineText_Edit.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Text.ascx", BuildPath ("DynamicData/FieldTemplates/Text.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Text.ascx.cs", BuildPath ("DynamicData/FieldTemplates/Text.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Text_Edit.ascx", BuildPath ("DynamicData/FieldTemplates/Text_Edit.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Text_Edit.ascx.cs", BuildPath ("DynamicData/FieldTemplates/Text_Edit.ascx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Details.aspx", BuildPath ("DynamicData/PageTemplates/Details.aspx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Details.aspx.cs", BuildPath ("DynamicData/PageTemplates/Details.aspx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Edit.aspx", BuildPath ("DynamicData/PageTemplates/Edit.aspx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Edit.aspx.cs", BuildPath ("DynamicData/PageTemplates/Edit.aspx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Insert.aspx", BuildPath ("DynamicData/PageTemplates/Insert.aspx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Insert.aspx.cs", BuildPath ("DynamicData/PageTemplates/Insert.aspx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.List.aspx", BuildPath ("DynamicData/PageTemplates/List.aspx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.List.aspx.cs", BuildPath ("DynamicData/PageTemplates/List.aspx.cs"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.ListDetails.aspx", BuildPath ("DynamicData/PageTemplates/ListDetails.aspx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.ListDetails.aspx.cs", BuildPath ("DynamicData/PageTemplates/ListDetails.aspx.cs"));

			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.CustomFieldTemplate.ascx", BuildPath ("DynamicData/FieldTemplates/CustomFieldTemplate.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.CustomFieldTemplate.ascx.cs", BuildPath ("DynamicData/FieldTemplates/CustomFieldTemplate.ascx.cs"));

			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.MyCustomUIHintTemplate_Text.ascx", BuildPath ("DynamicData/FieldTemplates/MyCustomUIHintTemplate_Text.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.MyCustomUIHintTemplate_Text.ascx.cs", BuildPath ("DynamicData/FieldTemplates/MyCustomUIHintTemplate_Text.ascx.cs"));

			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.PlainControlTemplate.ascx", BuildPath ("DynamicData/FieldTemplates/PlainControlTemplate.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.PlainControlTemplate.ascx.cs", BuildPath ("DynamicData/FieldTemplates/PlainControlTemplate.ascx.cs"));

			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.CustomColor.ascx", BuildPath ("DynamicData/FieldTemplates/CustomColor.ascx"));
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.CustomColor.ascx.cs", BuildPath ("DynamicData/FieldTemplates/CustomColor.ascx.cs"));
		}
	}
}
