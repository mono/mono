//
// System.Web.UI.Page.jsf.cs
//
// Authors:
//   Igor Zelmanovich (igorz@mainsoft.com)
//
// (C) 2007 Mainsoft Co. (http://www.mainsoft.com)
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

using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.component;
using java.util;
using javax.faces.el;
using javax.faces.@event;
using javax.faces.context;

namespace System.Web.UI
{
	public partial class Control : UIComponentBase
	{

		#region Forward to base

		public override Map getAttributes () {
			return base.getAttributes ();
		}

		public override ValueBinding getValueBinding (string name) {
			return base.getValueBinding (name);
		}

		public override void setValueBinding (string name, ValueBinding binding) {
			base.setValueBinding (name, binding);
		}

		public override Map getFacets () {
			return base.getFacets ();
		}

		public override UIComponent getFacet (string name) {
			return base.getFacet (name);
		}

		public override void broadcast (FacesEvent @event) {
			base.broadcast (@event);
		}

		[MonoTODO ("is this correct")]
		public override void decode (FacesContext context) {
			base.decode (context);
		}

		[MonoTODO ("should integrate this")]
		public override void encodeBegin (FacesContext context) {
			base.encodeBegin (context);
		}

		[MonoTODO ("should integrate this")]
		public override void encodeEnd (FacesContext context) {
			base.encodeEnd (context);
		}

		[MonoTODO ("should integrate this")]
		public override object saveState (FacesContext context) {
			return base.saveState (context);
		}

		[MonoTODO ("should integrate this")]
		public override void restoreState (FacesContext context, object state) {
			base.restoreState (context, state);
		}

		#endregion

		//kostat should use set
		public override bool isTransient () {
			return true;
		}



		//public Control () {
		//}

		[MonoTODO ("shouldn't be the reverse?")]
		public override string getClientId (FacesContext context) {
			return ClientID;
		}

		[MonoTODO ("shouldn't be the reverse?")]
		public override string getId () {
			return ID;
		}

		[MonoTODO ("shouldn't be the reverse?")]
		public override void setId (string id) {
			ID = id;
		}

		[MonoTODO ("shouldn't be the reverse?")]
		public override UIComponent getParent () {
			//return Parent;
			return base.getParent ();
		}

		[MonoTODO ("shouldn't be the reverse?")]
		public override void setParent (UIComponent parent) {
			Control p = parent as Control;
			if (p != null)
				p.Controls.Add (this);
			//throw new NotImplementedException("parent is not a Control"); //need to write a wrapper

			//Wrong, wrong code!!
			base.setParent (parent);
		}

		public override bool getRendersChildren () {
			return true; //see Control.Render
		}

		public override List getChildren () {
			//throw new NotImplementedException (); //should actually return Controls collection
			return base.getChildren ();
		}

		public override int getChildCount () {
			//throw new NotImplementedException (); //should actually return Controls.Count
			return base.getChildCount ();
		}

		public override UIComponent findComponent (string expr) {
			return FindControl (expr);
		}

		public override Iterator getFacetsAndChildren () {
			//throw new NotImplementedException (); //should actually return Controls collection
			return base.getFacetsAndChildren ();
		}

		[MonoTODO ("should integrate this, i.e. reverse call")]
		public override void encodeChildren (FacesContext context) {

			RenderControl (new HtmlTextWriter (Context.Response.Output));
			//throw new NotImplementedException (); //should actually get the writer
			//context.
			//base.encodeChildren (context.getResponseWriter);
		}

		public override string getFamily () {
			return null;
		}
	}
}
