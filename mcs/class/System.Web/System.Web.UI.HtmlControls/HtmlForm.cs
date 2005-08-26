//
// System.Web.UI.HtmlControls.HtmlForm.cs
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
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

using System.ComponentModel;
using System.Collections.Specialized;

namespace System.Web.UI.HtmlControls 
{
	public class HtmlForm : HtmlContainerControl 
	{
		public HtmlForm () : base ("form")
		{
		}

#if NET_2_0
		[DefaultValue ("")]
		[MonoTODO]
		public string DefaultButton
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue ("")]
		[MonoTODO]
		public string DefaultFocus
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif		

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Enctype 
		{
			get {
				string enc = Attributes["enctype"];

				if (enc == null) {
					return (String.Empty);
				}

				return (enc);
			}
			set {
				if (value == null) {
					Attributes.Remove ("enctype");
				} else {
					Attributes["enctype"] = value;
				}
			}
		}
		
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Method 
		{
			get {
				string method = Attributes["method"];

				if (method == null) {
					return ("post");
				}
				
				return (method);
			}
			set {
				if (value == null) {
					Attributes.Remove ("method");
				} else {
					Attributes["method"] = value;
				}
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		public virtual
#else		
		public
#endif		
		string Name 
		{
			get {
				string name = Attributes["name"];

				if (name == null) {
					return (UniqueID);
				}
				
				return (name);
			}
			set {
				if (value == null) {
					Attributes.Remove ("name");
				} else {
					Attributes["name"] = value;
				}
			}
		}

#if NET_2_0
		[DefaultValue (false)]
		[MonoTODO]
		public virtual bool SubmitDisabledControls 
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif
			
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Target 
		{
			get {
				string target = Attributes["target"];

				if (target == null) {
					return (String.Empty);
				}
				
				return (target);
			}
			set {
				if (value == null) {
					Attributes.Remove ("target");
				} else {
					Attributes["target"] = value;
				}
			}
		}

#if NET_2_0
		public override
#else		
		// New in NET1.1 sp1
		public new
#endif		
		string UniqueID
		{
			get {
				return base.UniqueID;
			}
		}

#if NET_2_0		
		[MonoTODO]
		protected override ControlCollection CreateControlCollection ()
		{
			return base.CreateControlCollection ();
		}
#endif		

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnInit (EventArgs e)
		{
			Page.RegisterViewStateHandler ();

			base.OnInit (e);
		}

#if NET_2_0
		[MonoTODO("Probably something about validators here")]
		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender(e);
		}
#endif		

		protected override void RenderAttributes (HtmlTextWriter w)
		{
			/* Need to always render: name, method, action
			 * and id
			 */

			string action = Page.Request.FilePath;
			string query = Page.Request.QueryStringRaw;
			if (query != null && query.Length > 0) {
				action += "?" + query;
			}
			
			w.WriteAttribute ("name", Name);
			w.WriteAttribute ("method", Method);
			w.WriteAttribute ("action", action);

			if (ID == null) {
				/* If ID != null then HtmlControl will
				 * write the id attribute
				 */
				w.WriteAttribute ("id", ClientID);
				Attributes.Remove ("id");
			}

			string submit = Page.GetSubmitStatements ();
			if (submit != null && submit != "")
				w.WriteAttribute ("onsubmit", submit);
			
			/* enctype and target should not be written if
			 * they are empty
			 */
			string enctype = Enctype;
			if (enctype != null && enctype != "") {
				w.WriteAttribute ("enctype", enctype);
			}

			string target = Target;
			if (target != null && target != "") {
				w.WriteAttribute ("target", target);
			}
			
			/* Now remove them from the hash so the base
			 * RenderAttributes can do all the rest
			 */
			Attributes.Remove ("name");
			Attributes.Remove ("method");
			Attributes.Remove ("enctype");
			Attributes.Remove ("target");

			base.RenderAttributes (w);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderChildren (HtmlTextWriter w)
		{
			Page.OnFormRender (w, ClientID);
			base.RenderChildren (w);
			Page.OnFormPostRender (w, ClientID);
		}

#if NET_2_0
		/* According to corcompare */
		[MonoTODO]
		public override void RenderControl (HtmlTextWriter w)
		{
			base.RenderControl (w);
		}
#endif		

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter w)
		{
			base.Render (w);
		}
	}
}

	
