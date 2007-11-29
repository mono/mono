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
	public partial class Control : UIComponent, StateHolder
	{
		ComponentChildrenList _childrenList;

		protected override void addFacesListener (FacesListener __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void broadcast (FacesEvent __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void decode (FacesContext context) {
			// do nothing
		}

		public override void encodeBegin (FacesContext __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void encodeChildren (FacesContext __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void encodeEnd (FacesContext __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override UIComponent findComponent (string __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override Map getAttributes () {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override int getChildCount () {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override List getChildren () {
			if (_childrenList == null)
				_childrenList = new ComponentChildrenList (this);
			return _childrenList;
		}

		public override string getClientId (FacesContext __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		protected override FacesContext getFacesContext () {
			return FacesContext.getCurrentInstance ();
		}

		protected override FacesListener [] getFacesListeners (java.lang.Class __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override UIComponent getFacet (string __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override Map getFacets () {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override Iterator getFacetsAndChildren () {
			// TODO: consider facets.
			return getChildren ().iterator ();
		}

		public override string getFamily () {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override string getId () {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override UIComponent getParent () {
			throw new Exception ("The method or operation is not implemented.");
		}

		protected override javax.faces.render.Renderer getRenderer (FacesContext __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override string getRendererType () {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override bool getRendersChildren () {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override ValueBinding getValueBinding (string name) {
			return null;
		}

		public override bool isRendered () {
			throw new Exception ("The method or operation is not implemented.");
		}

		// TODO: implement
		public override void processDecodes (FacesContext context) {
			// call processDecodes for all jsf children
		}

		public override void processRestoreState (FacesContext context, object state) {
			throw new NotSupportedException ();
		}

		public override object processSaveState (FacesContext context) {
			throw new NotSupportedException ();
		}

		// TODO: implement
		public override void processUpdates (FacesContext context) {
			// call processUpdates for all jsf children
		}

		// TODO: implement
		public override void processValidators (FacesContext context) {
			// call processValidators for all jsf children
		}

		public override void queueEvent (FacesEvent __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		protected override void removeFacesListener (FacesListener __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void setId (string __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void setParent (UIComponent __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void setRendered (bool __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void setRendererType (string __p1) {
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void setValueBinding (string name, ValueBinding binding) {
			throw new NotSupportedException ();
		}

		#region StateHolder Members

		bool StateHolder.isTransient () {
			return !EnableViewState;
		}

		void StateHolder.restoreState (FacesContext context, object state) {
			LoadViewState (state);
		}

		object StateHolder.saveState (FacesContext context) {
			return SaveViewState ();
		}

		void StateHolder.setTransient (bool newTransientValue) {
			EnableViewState = !newTransientValue;
		}

		#endregion

		class ComponentChildrenList : AbstractList
		{
			Control _owner;

			public ComponentChildrenList (Control owner) {
				_owner = owner;
			}

			public override object get (int index) {
				return _owner.Controls [index];
			}

			public override int size () {
				return _owner.Controls.Count;
			}
		}
	}
}
