//
// System.Web.UI.BasePartialCachingControl.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Andreas Nahr
// (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Web.Caching;

namespace System.Web.UI
{
	[ToolboxItem (false)]
	public abstract class BasePartialCachingControl : Control
	{
		private CacheDependency dependency;
		private string ctrl_id;
		private string guid;
		private int duration;
		private string varyby_params;
		private string varyby_controls;
		private string varyby_custom;

		private Control control;
		
		protected BasePartialCachingControl()
		{
		}

		internal string CtrlID {
			get { return ctrl_id; }
			set { ctrl_id = value; }
		}

		internal string Guid {
			get { return guid; }
			set { guid = value; }
		}

		internal int Duration {
			get { return duration; }
			set { duration = value; }
		}

		internal string VaryByParams {
			get { return varyby_params; }
			set { varyby_params = value; }
		}

		internal string VaryByControls {
			get { return varyby_controls; }
			set { varyby_controls = value; }
		}

		internal string VaryByCustom {
			get { return varyby_custom; }
			set { varyby_custom = value; }
		}

		internal abstract Control CreateControl ();

		public override void Dispose ()
		{
			if (dependency != null) {
				dependency.Dispose ();
				dependency = null;
			}
		}

		protected override void OnInit (EventArgs e)
		{
			control = CreateControl ();
			Controls.Add (control);
		}

		protected override void Render (HtmlTextWriter output)
		{
			Cache cache = HttpRuntime.Cache;
			string key = CreateKey ();
			string data = cache [key] as string;

			if (data != null) {
				output.Write (data);
				return;
			}

			HttpContext context = HttpContext.Current;
			StringWriter writer = new StringWriter ();
			TextWriter prev = context.Response.SetTextWriter (writer);
			HtmlTextWriter txt_writer = new HtmlTextWriter (writer);
			string text;
			try {
				control.RenderControl (txt_writer);
			} finally {
				text = writer.ToString ();
				context.Response.SetTextWriter (prev);
				output.Write (text);
			}

			context.Cache.InsertPrivate (key, text, dependency,
						DateTime.Now.AddSeconds (duration),
						Cache.NoSlidingExpiration,
						CacheItemPriority.Normal, null);
		}

		public CacheDependency Dependency {
			get {return dependency;}
			set {dependency = value;}
		}

		private string CreateKey ()
		{
			StringBuilder builder = new StringBuilder ();
			HttpContext context = HttpContext.Current;

			builder.Append ("PartialCachingControl\n");
			builder.Append ("GUID: " + guid + "\n");

			if (varyby_params != null && varyby_params.Length > 0) {
				string[] prms = varyby_params.Split (';');
				for (int i=0; i<prms.Length; i++) {
					string val = context.Request.Params [prms [i]];
					builder.Append ("VP:");
					builder.Append (prms [i]);
					builder.Append ('=');
					builder.Append (val != null ? val : "__null__");
					builder.Append ('\n');
				}
			}

			if (varyby_controls != null && varyby_params.Length > 0) {
				string[] prms = varyby_controls.Split (';');
				for (int i=0; i<prms.Length; i++) {
					string val = context.Request.Params [prms [i]];
					builder.Append ("VCN:");
					builder.Append (prms [i]);
					builder.Append ('=');
					builder.Append (val != null ? val : "__null__");
					builder.Append ('\n');
				}
			}

			if (varyby_custom != null) {
				string val = context.ApplicationInstance.GetVaryByCustomString (context,
						varyby_custom);
				builder.Append ("VC:");
				builder.Append (varyby_custom);
				builder.Append ('=');
				builder.Append (val != null ? val : "__null__");
				builder.Append ('\n');
			}

			return builder.ToString ();
		}
	}
}

