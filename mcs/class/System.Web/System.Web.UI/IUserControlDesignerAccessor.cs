//
// System.Web.UI.IUserControlDesignerAccessor.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.Web.UI
{
	public interface IUserControlDesignerAccessor
        {
                string InnerText { get; set; }
		string TagName { get; set; }
        }
}
