//
// System.Web.UI.BasePartialCachingControl.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;
using System.Web.Caching;

namespace System.Web.UI
{
	[ToolboxItem (false)]
	public abstract class BasePartialCachingControl : Control
	{
		private CacheDependency dependency;

		protected BasePartialCachingControl()
		{
		}

		internal abstract Control CreateControl ();

		[MonoTODO]
		public override void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnInit (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Render (HtmlTextWriter output)
		{
			throw new NotImplementedException ();
		}

		public CacheDependency Dependency {
			get {return dependency;}
			set {dependency = value;}
		}
	}
}
