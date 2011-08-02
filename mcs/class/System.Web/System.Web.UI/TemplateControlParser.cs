//
// System.Web.UI.TemplateControlParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.Web.Hosting;
using System.Web.Util;

namespace System.Web.UI
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class TemplateControlParser : BaseTemplateParser 
	{
		bool autoEventWireup = true;
		bool enableViewState = true;
		CompilationMode compilationMode = CompilationMode.Always;
#if NET_4_0
		ClientIDMode? clientIDMode;
#endif
		TextReader reader;

		protected TemplateControlParser ()
		{
			LoadConfigDefaults ();
		}
		
		internal override void LoadConfigDefaults ()
		{
			base.LoadConfigDefaults ();
			PagesSection ps = PagesConfig;
			autoEventWireup = ps.AutoEventWireup;
			enableViewState = ps.EnableViewState;
			compilationMode = ps.CompilationMode;
		}
		
		internal override void ProcessMainAttributes (IDictionary atts)
		{
			autoEventWireup = GetBool (atts, "AutoEventWireup", autoEventWireup);
			enableViewState = GetBool (atts, "EnableViewState", enableViewState);

			string value = GetString (atts, "CompilationMode", compilationMode.ToString ());
			if (!String.IsNullOrEmpty (value)) {
				try {
					compilationMode = (CompilationMode) Enum.Parse (typeof (CompilationMode), value, true);
				} catch (Exception ex) {
					ThrowParseException ("Invalid value of the CompilationMode attribute.", ex);
				}
			}
			
			atts.Remove ("TargetSchema"); // Ignored
#if NET_4_0
			value = GetString (atts, "ClientIDMode", null);
			if (!String.IsNullOrEmpty (value)) {
				try {
					clientIDMode = (ClientIDMode) Enum.Parse (typeof (ClientIDMode), value, true);
				} catch (Exception ex) {
					ThrowParseException ("Invalid value of the ClientIDMode attribute.", ex);
				}
			}
#endif
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
				
				string virtualPath = GetString (atts, "VirtualPath", null);
				if (virtualPath != null)
					if (vp != null)
						dupe = true;
					else
						vp = virtualPath;
				
				if (vp == null)
					ThrowParseException ("Must provide one of the 'page', 'control' or 'virtualPath' attributes");
				
				if (dupe)
					ThrowParseException ("Only one attribute can be specified.");

				vp = HostingEnvironment.VirtualPathProvider.CombineVirtualPaths (VirtualPath.Absolute, vp);
				AddDependency (vp, false);
				
				Type ctype;
				ctype = BuildManager.GetCompiledType (vp);
				
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
		
		internal CompilationMode CompilationMode {
			get { return compilationMode; }
		}		
#if NET_4_0
		internal ClientIDMode? ClientIDMode {
			get { return clientIDMode; }
		}
#endif
		internal override TextReader Reader {
			get { return reader; }
			set { reader = value; }
		}
	}
}

