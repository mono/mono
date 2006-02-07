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
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Web.Compilation;
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

		protected TemplateControlParser ()
		{
		}

		internal override void ProcessMainAttributes (Hashtable atts)
		{
			autoEventWireup = GetBool (atts, "AutoEventWireup", PagesConfig.AutoEventWireup);
			enableViewState = GetBool (atts, "EnableViewState", PagesConfig.EnableViewState);

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

		internal override void AddDirective (string directive, Hashtable atts)
		{
			int cmp = String.Compare ("Register", directive, true);
			if (cmp == 0) {
				string tagprefix = GetString (atts, "TagPrefix", null);
				if (tagprefix == null || tagprefix.Trim () == "")
					ThrowParseException ("No TagPrefix attribute found.");

				string ns = GetString (atts, "Namespace", null);
				string assembly = GetString (atts, "Assembly", null);

				if (ns != null && assembly == null)
					ThrowParseException ("Need an Assembly attribute with Namespace.");

				if (ns == null && assembly != null)
					ThrowParseException ("Need a Namespace attribute with Assembly.");
				
				if (ns != null) {
					if (atts.Count != 0)
						ThrowParseException ("Unknown attribute: " + GetOneKey (atts));

					AddImport (ns);
					Assembly ass = AddAssemblyByName (assembly);
					AddDependency (ass.Location);
					RootBuilder.Foundry.RegisterFoundry (tagprefix, ass, ns);
					return;
				}

				string tagname = GetString (atts, "TagName", null);
				string src = GetString (atts, "Src", null);

				if (tagname == null && src != null)
					ThrowParseException ("Need a TagName attribute with Src.");

				if (tagname != null && src == null)
					ThrowParseException ("Need a Src attribute with TagName.");

				if (!src.EndsWith (".ascx"))
					ThrowParseException ("Source file extension for controls must be .ascx");

				string realpath = MapPath (src);
				if (!File.Exists (realpath))
					throw new ParseException (Location, "Could not find file \"" 
						+ realpath + "\".");

				string vpath = UrlUtils.Combine (BaseVirtualDir, src);
				Type type = null;
				try {
					type = UserControlParser.GetCompiledType (vpath, realpath, Dependencies, Context);
				} catch (ParseException pe) {
					if (this is UserControlParser)
						throw new ParseException (Location, pe.Message, pe);
					throw;
				}

				AddAssembly (type.Assembly, true);
				RootBuilder.Foundry.RegisterFoundry (tagprefix, tagname, type);
				return;
			}

			cmp = String.Compare ("Reference", directive, true);
			if (cmp == 0) {
				string page = GetString (atts, "Page", null);
				string control = GetString (atts, "Control", null);

				bool is_page = (page != null);
				if (!is_page && control == null)
					ThrowParseException ("Must provide 'page' or 'control' attribute");

				if (is_page && control != null)
					ThrowParseException ("'page' and 'control' are mutually exclusive");

				string filepath = (!is_page) ? control : page;
				filepath = MapPath (filepath);
				AddDependency (filepath);
				Type ctype;
				if (is_page) {
					PageParser pp = new PageParser (page, filepath, Context);
					ctype = pp.CompileIntoType ();
				} else {
					ctype = UserControlParser.GetCompiledType (control, filepath, Dependencies, Context);
				}

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
	}
}

