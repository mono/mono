//
// System.Web.UI.TemplateControlParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.UI {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class TemplateControlParser
#if NET_2_0
		: BaseTemplateParser 
#else
		: TemplateParser
#endif
	{

		bool autoEventWireup = true;
		bool enableViewState = true;
#if NET_2_0
		CompilationMode compilationMode = CompilationMode.Always;
		TextReader reader;
#endif

		protected TemplateControlParser ()
		{
			LoadConfigDefaults ();
		}
		
		internal override void LoadConfigDefaults ()
		{
			base.LoadConfigDefaults ();
#if NET_2_0
			PagesSection ps = PagesConfig;
#else
			PagesConfiguration ps = PagesConfig;
#endif

#if NET_1_1
			autoEventWireup = ps.AutoEventWireup;
			enableViewState = ps.EnableViewState;
#endif
#if NET_2_0
			compilationMode = ps.CompilationMode;
#endif
		}
		
		internal override void ProcessMainAttributes (IDictionary atts)
		{
			autoEventWireup = GetBool (atts, "AutoEventWireup", autoEventWireup);
			enableViewState = GetBool (atts, "EnableViewState", enableViewState);
#if NET_2_0
			string cmode = GetString (atts, "CompilationMode", compilationMode.ToString ());
			if (!String.IsNullOrEmpty (cmode)) {
				if (String.Compare (cmode, "always", StringComparison.InvariantCultureIgnoreCase) == 0)
					compilationMode = CompilationMode.Always;
				else if (String.Compare (cmode, "auto", StringComparison.InvariantCultureIgnoreCase) == 0)
					compilationMode = CompilationMode.Auto;
				else if (String.Compare (cmode, "never", StringComparison.InvariantCultureIgnoreCase) == 0)
					compilationMode = CompilationMode.Never;
				else
					ThrowParseException ("Invalid value of the CompilationMode attribute");
			}
#endif
			atts.Remove ("TargetSchema"); // Ignored

			base.ProcessMainAttributes (atts);
		}

		internal object GetCompiledInstance ()
		{
			Type type = CompileIntoType ();
			if (type == null)
				return null;

			object ctrl = Activator.CreateInstance (type);
			if (ctrl == null)
				return null;

			HandleOptions (ctrl);
			return ctrl;
		}

		internal override void AddDirective (string directive, IDictionary atts)
		{
			int cmp = String.Compare ("Register", directive, true, Helpers.InvariantCulture);
			if (cmp == 0) {
				string tagprefix = GetString (atts, "TagPrefix", null);
				if (tagprefix == null || tagprefix.Trim () == "")
					ThrowParseException ("No TagPrefix attribute found.");

				string ns = GetString (atts, "Namespace", null);
				string assembly = GetString (atts, "Assembly", null);

#if !NET_2_0
				if (ns != null && assembly == null)
					ThrowParseException ("Need an Assembly attribute with Namespace.");
#endif
				
				if (ns == null && assembly != null)
					ThrowParseException ("Need a Namespace attribute with Assembly.");
				
				if (ns != null) {
					if (atts.Count != 0)
						ThrowParseException ("Unknown attribute: " + GetOneKey (atts));

					RegisterNamespace (tagprefix, ns, assembly);
					return;
				}

				string tagname = GetString (atts, "TagName", null);
				string src = GetString (atts, "Src", null);

				if (tagname == null && src != null)
					ThrowParseException ("Need a TagName attribute with Src.");

				if (tagname != null && src == null)
					ThrowParseException ("Need a Src attribute with TagName.");

#if !NET_2_0
				if (!StrUtils.EndsWith (src, ".ascx", true))
					ThrowParseException ("Source file extension for controls must be .ascx");
#endif
				
				RegisterCustomControl (tagprefix, tagname, src);
				return;
			}

			cmp = String.Compare ("Reference", directive, true, Helpers.InvariantCulture);
			if (cmp == 0) {
				string vp = null;
				string page = GetString (atts, "Page", null);
				bool is_page = (page != null);

				if (is_page)
					vp = page;

				bool dupe = false;
				string control = GetString (atts, "Control", null);
				if (control != null)
					if (is_page)
						dupe = true;
					else
						vp = control;
				
#if NET_2_0
				string virtualPath = GetString (atts, "VirtualPath", null);
				if (virtualPath != null)
					if (vp != null)
						dupe = true;
					else
						vp = virtualPath;
#endif
				
				if (vp == null) {
#if NET_2_0
					ThrowParseException ("Must provide one of the 'page', 'control' or 'virtualPath' attributes");
#else
					ThrowParseException ("Must provide one of the 'page' or 'control' attributes");
#endif
				}
				
				if (dupe)
					ThrowParseException ("Only one attribute can be specified.");

				AddDependency (vp);
				
				Type ctype;
#if NET_2_0
				ctype = BuildManager.GetCompiledType (vp);
#else
				string filepath = MapPath (vp);
				if (is_page) {
					PageParser pp = new PageParser (page, filepath, Context);
					ctype = pp.CompileIntoType ();
				} else {
					ctype = UserControlParser.GetCompiledType (vp, filepath, Dependencies, Context);
				}
#endif
				
				AddAssembly (ctype.Assembly, true);
				if (atts.Count != 0)
					ThrowParseException ("Unknown attribute: " + GetOneKey (atts));

				return;
			}

			base.AddDirective (directive, atts);
		}

		internal override void HandleOptions (object obj)
		{
			base.HandleOptions (obj);

			Control ctrl = obj as Control;
			ctrl.AutoEventWireup = autoEventWireup;
			ctrl.EnableViewState = enableViewState;
		}

		internal bool AutoEventWireup {
			get { return autoEventWireup; }
		}

		internal bool EnableViewState {
			get { return enableViewState; }
		}
		
#if NET_2_0
		internal CompilationMode CompilationMode {
			get { return compilationMode; }
		}
		
		internal override TextReader Reader {
			get { return reader; }
			set { reader = value; }
		}
#endif
	}
}

