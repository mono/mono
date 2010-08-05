//
// System.Web.UI.BasePartialCachingControl.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Andreas Nahr
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.IO;
using System.Text;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web.Caching;

namespace System.Web.UI
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ToolboxItem (false)]
	public abstract class BasePartialCachingControl : Control
	{
		CacheDependency dependency;
		string ctrl_id;
		string guid;
		int duration;
		string varyby_params;
		string varyby_controls;
		string varyby_custom;
		DateTime expirationTime;
		bool slidingExpiration;
		
		Control control;
		ControlCachePolicy cachePolicy;
		string cacheKey;
		string cachedData;
		
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

		internal DateTime ExpirationTime {
			get { return expirationTime; }
			set { expirationTime = value; }
		}

		internal bool SlidingExpiration {
			get { return slidingExpiration; }
			set { slidingExpiration = value; }
		}
#if NET_4_0
		internal string ProviderName {
			get; set;
		}
#endif
		internal abstract Control CreateControl ();

		public override void Dispose ()
		{
			if (dependency != null) {
				dependency.Dispose ();
				dependency = null;
			}
		}

		void RetrieveCachedContents ()
		{
			cacheKey = CreateKey ();
#if NET_4_0
			OutputCacheProvider provider = GetProvider ();
			cachedData = provider.Get (cacheKey) as string;
#else
			Cache cache = HttpRuntime.InternalCache;
			cachedData = cache [cacheKey] as string;
#endif
		}
#if NET_4_0
		OutputCacheProvider GetProvider ()
		{
			string providerName = ProviderName;
			OutputCacheProvider provider;

			if (String.IsNullOrEmpty (providerName))
				provider = OutputCache.DefaultProvider;
			else {
				provider = OutputCache.GetProvider (providerName);
				if (provider == null)
					provider = OutputCache.DefaultProvider;
			}

			return provider;
		}
		
		void OnDependencyChanged (string key, object value, CacheItemRemovedReason reason)
		{
			Console.WriteLine ("{0}.OnDependencyChanged (\"{0}\", {1}, {2})", this, key, value, reason);
			GetProvider ().Remove (key);
		}
		
		internal override void InitRecursive (Control namingContainer)
		{
			RetrieveCachedContents ();
			if (cachedData == null) {
				control = CreateControl ();
				Controls.Add (control);
			} else
				control = null;
			
			base.InitRecursive (namingContainer);
		}
#else
		protected internal override void OnInit (EventArgs e)
		{
			control = CreateControl ();
			Controls.Add (control);
		}
#endif
		protected internal override void Render (HtmlTextWriter output)
		{
#if !NET_4_0
			RetrieveCachedContents ();
#endif
			if (cachedData != null) {
				output.Write (cachedData);
				return;
			}

			if (control == null) {
				base.Render (output);
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
#if NET_4_0
			OutputCacheProvider provider = GetProvider ();
			DateTime utcExpire = DateTime.UtcNow.AddSeconds (duration);
			provider.Set (cacheKey, text, utcExpire);;
			context.InternalCache.Insert (cacheKey, text, dependency, utcExpire.ToLocalTime (),
						      Cache.NoSlidingExpiration, CacheItemPriority.Normal,
						      null);
#else
			context.InternalCache.Insert (cacheKey, text, dependency,
						      DateTime.Now.AddSeconds (duration),
						      Cache.NoSlidingExpiration,
						      CacheItemPriority.Normal, null);
#endif
		}

		public ControlCachePolicy CachePolicy 
		{
			get {
				if (cachePolicy == null)
					cachePolicy = new ControlCachePolicy (this);

				return cachePolicy;
			}
		}

		public CacheDependency Dependency {
			get {return dependency;}
			set {dependency = value;}
		}

		string CreateKey ()
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

