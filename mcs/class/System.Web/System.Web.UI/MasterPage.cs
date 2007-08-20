//
// System.Web.UI.MasterPage.cs
//
// Authors:
//   Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc.
//

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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Util;
using System.IO;

namespace System.Web.UI
{
	[ParseChildren (false)]
#if notyet
	[Designer (...)]
#endif
	[ControlBuilder (typeof (MasterPageControlBuilder))]
	public class MasterPage: UserControl
	{
		Hashtable definedContentTemplates = new Hashtable ();
		Hashtable templates = new Hashtable ();
		ArrayList placeholders = new ArrayList ();
		List <string> placeholderIds;
		
		string parentMasterPageFile = null;
		MasterPage parentMasterPage;

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal void AddContentTemplate (string templateName, ITemplate template)
		{
			// LAMESPEC: should be ArgumentException
			if (definedContentTemplates.ContainsKey (templateName))
				throw new HttpException ("Multiple contents applied to " + templateName);

			definedContentTemplates [templateName] = template;
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal IList ContentPlaceHolders {
			get { return placeholders; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal IDictionary ContentTemplates {
			get { return templates; }
		}
		
		[DefaultValueAttribute ("")]
		public string MasterPageFile {
			get { return parentMasterPageFile; }
			set {
				parentMasterPageFile = value;
				parentMasterPage = null;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public MasterPage Master {
			get {
				if (parentMasterPage == null && parentMasterPageFile != null)
					parentMasterPage = MasterPage.CreateMasterPage (this, Context, parentMasterPageFile, definedContentTemplates);
			
				return parentMasterPage;
			}
		}
		
		internal void SetPlaceHolderIds (List <string> ids)
		{
			placeholderIds = ids;
		}
		
		internal static MasterPage CreateMasterPage (TemplateControl owner, HttpContext context,
							     string masterPageFile, IDictionary contentTemplateCollection)
		{
			MasterPage masterPage = MasterPageParser.GetCompiledMasterInstance (masterPageFile,
											    owner.Page.MapPath (masterPageFile),
											    context);
			List <string> phids = masterPage.placeholderIds;			
			if (contentTemplateCollection != null) {
				foreach (string templateName in contentTemplateCollection.Keys) {
					if (phids != null && !phids.Contains (templateName))
						throw new HttpException (
							String.Format ("Cannot find ContentPlaceHolder '{0}' in the master page '{1}'",
								templateName, masterPageFile));
					if (masterPage.ContentTemplates [templateName] == null)
						masterPage.ContentTemplates [templateName] = contentTemplateCollection[templateName];
				}
			}
			masterPage.Page = owner.Page;
			masterPage.InitializeAsUserControlInternal ();
			return masterPage;
		}

		internal static void ApplyMasterPageRecursive (MasterPage master, IList appliedMasterPageFiles)
		{
			/* XXX need to use virtual paths here? */
			if (master.MasterPageFile != null) {
				if (appliedMasterPageFiles.Contains (master.MasterPageFile))
					throw new HttpException ("circular dependency in master page files detected");
				if (master.Master != null) {
					master.Controls.Clear ();
					master.Controls.Add (master.Master);
					appliedMasterPageFiles.Add (master.MasterPageFile);
					MasterPage.ApplyMasterPageRecursive (master.Master, appliedMasterPageFiles);
				}
			}
		}
	}
}

#endif
