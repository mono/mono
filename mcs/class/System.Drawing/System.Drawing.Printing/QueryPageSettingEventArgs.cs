//
// System.Drawing.QueryPageSettingEventArgs.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing.Printing
{
	/// <summary>
	/// Summary description for QueryPageSettingEventArgs.
	/// </summary>
	public class QueryPageSettingEventArgs : PrintEventArgs
	{
		private PageSettings pageSettings;

		public QueryPageSettingEventArgs(PageSettings pageSettings)
		{
			this.pageSettings = pageSettings;
		}
		public PageSettings PageSettings {
			get{
			return pageSettings;
		}
			set{
				pageSettings = value;
			}
		}

	}
}
