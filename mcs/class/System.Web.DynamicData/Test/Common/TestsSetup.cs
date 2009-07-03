using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTests.SystemWeb.Framework;

namespace MonoTests.Common
{
	public static class TestsSetup
	{
		public static void CopyResources ()
		{
			Type type = typeof (TestsSetup);
			WebTest.CopyResource (type, "MonoTests.WebPages.Global.asax", "Global.asax");
			WebTest.CopyResource (type, "MonoTests.WebPages.web.config", "web.config");
			WebTest.CopyResource (type, "MonoTests.WebPages.Site.css", "Site.css");
			WebTest.CopyResource (type, "MonoTests.WebPages.Site.master", "Site.master");
			WebTest.CopyResource (type, "MonoTests.WebPages.Site.master.cs", "Site.master.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.web.config", "DynamicData/web.config");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.FilterUserControl.ascx", "DynamicData/Content/FilterUserControl.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.FilterUserControl.ascx.cs", "DynamicData/Content/FilterUserControl.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.GridViewPager.ascx", "DynamicData/Content/GridViewPager.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.GridViewPager.ascx.cs", "DynamicData/Content/GridViewPager.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.Back.gif", "DynamicData/Content/Images/Back.gif");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.header_back.gif", "DynamicData/Content/Images/header_back.gif");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.PgFirst.gif", "DynamicData/Content/Images/PgFirst.gif");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.PgLast.gif", "DynamicData/Content/Images/PgLast.gif");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.PgNext.gif", "DynamicData/Content/Images/PgNext.gif");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.PgPrev.gif", "DynamicData/Content/Images/PgPrev.gif");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.Content.Images.plus.gif", "DynamicData/Content/Images/plus.gif");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Boolean.ascx", "DynamicData/FieldTemplates/Boolean.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Boolean.ascx.cs", "DynamicData/FieldTemplates/Boolean.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Boolean_Edit.ascx", "DynamicData/FieldTemplates/Boolean_Edit.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Boolean_Edit.ascx.cs", "DynamicData/FieldTemplates/Boolean_Edit.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Children.ascx", "DynamicData/FieldTemplates/Children.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Children.ascx.cs", "DynamicData/FieldTemplates/Children.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.DateTime.ascx", "DynamicData/FieldTemplates/DateTime.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.DateTime.ascx.cs", "DynamicData/FieldTemplates/DateTime.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.DateTime_Edit.ascx", "DynamicData/FieldTemplates/DateTime_Edit.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.DateTime_Edit.ascx.cs", "DynamicData/FieldTemplates/DateTime_Edit.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Decimal_Edit.ascx", "DynamicData/FieldTemplates/Decimal_Edit.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Decimal_Edit.ascx.cs", "DynamicData/FieldTemplates/Decimal_Edit.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey.ascx", "DynamicData/FieldTemplates/ForeignKey.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey.ascx.cs", "DynamicData/FieldTemplates/ForeignKey.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey_Edit.ascx", "DynamicData/FieldTemplates/ForeignKey_Edit.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey_Edit.ascx.cs", "DynamicData/FieldTemplates/ForeignKey_Edit.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Integer_Edit.ascx", "DynamicData/FieldTemplates/Integer_Edit.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Integer_Edit.ascx.cs", "DynamicData/FieldTemplates/Integer_Edit.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.MultilineText_Edit.ascx", "DynamicData/FieldTemplates/MultilineText_Edit.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.MultilineText_Edit.ascx.cs", "DynamicData/FieldTemplates/MultilineText_Edit.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Text.ascx", "DynamicData/FieldTemplates/Text.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Text.ascx.cs", "DynamicData/FieldTemplates/Text.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Text_Edit.ascx", "DynamicData/FieldTemplates/Text_Edit.ascx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates.Text_Edit.ascx.cs", "DynamicData/FieldTemplates/Text_Edit.ascx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Details.aspx", "DynamicData/PageTemplates/Details.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Details.aspx.cs", "DynamicData/PageTemplates/Details.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Edit.aspx", "DynamicData/PageTemplates/Edit.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Edit.aspx.cs", "DynamicData/PageTemplates/Edit.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Insert.aspx", "DynamicData/PageTemplates/Insert.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.Insert.aspx.cs", "DynamicData/PageTemplates/Insert.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.List.aspx", "DynamicData/PageTemplates/List.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.List.aspx.cs", "DynamicData/PageTemplates/List.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.ListDetails.aspx", "DynamicData/PageTemplates/ListDetails.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.PageTemplates.ListDetails.aspx.cs", "DynamicData/PageTemplates/ListDetails.aspx.cs");
		}
	}
}
