//
// System.Web.UI.Design.WebFormsRootDesigner
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Drawing;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design
{
	public abstract class WebFormsRootDesigner : IRootDesigner, IDesigner, IDisposable, IDesignerFilter
	{
		protected WebFormsRootDesigner ()
		{
		}

		~WebFormsRootDesigner ()
		{
		}

		public event EventHandler LoadComplete;

		public abstract string DocumentUrl { get; }

		public abstract bool IsDesignerViewLocked { get; }

		public abstract bool IsLoading { get; }

		public abstract WebFormsReferenceManager ReferenceManager { get; }

		public abstract void AddClientScriptToDocument (ClientScriptItem scriptItem);

		public abstract string AddControlToDocument (Control newControl, Control referenceControl, ControlLocation location);

		public abstract ClientScriptItemCollection GetClientScriptsInDocument ();

		protected internal abstract void GetControlViewAndTag (Control control, out IControlDesignerView view, out IControlDesignerTag tag);

		public abstract void RemoveClientScriptFromDocument (string clientScriptId);

		public abstract void RemoveControlFromDocument (Control control);

		[MonoTODO]
		public virtual IComponent Component {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public CultureInfo CurrentCulture {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected ViewTechnology[] SupportedTechnologies {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected DesignerVerbCollection Verbs {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected virtual DesignerActionService CreateDesignerActionService (IServiceProvider serviceProvider)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual IUrlResolutionService CreateUrlResolutionService ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GenerateEmptyDesignTimeHtml (Control control)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GenerateErrorDesignTimeHtml (Control control, Exception e, string errorMessage)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual object GetService (Type serviceType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object GetView (ViewTechnology viewTechnology)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Initialize (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnLoadComplete (EventArgs e)
		{
			if (LoadComplete != null)
				LoadComplete (this, e);
		}

		[MonoTODO]
		protected virtual void PostFilterAttributes (IDictionary attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PostFilterEvents (IDictionary events)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PostFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PreFilterAttributes (IDictionary attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PreFilterEvents (IDictionary events)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string ResolveUrl (string relativeUrl)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetControlID (Control control, string id)
		{
			throw new NotImplementedException ();
		}

		// Explicit interface implementations

		[MonoTODO]
		DesignerVerbCollection IDesigner.Verbs {
			get { return Verbs; }
		}

		[MonoTODO]
		ViewTechnology [] IRootDesigner.SupportedTechnologies {
			get { return SupportedTechnologies; }
		}

		[MonoTODO]
		void IDesigner.DoDefaultAction ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDesignerFilter.PostFilterAttributes (IDictionary attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDesignerFilter.PostFilterEvents (IDictionary events)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDesignerFilter.PostFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDesignerFilter.PreFilterAttributes (IDictionary attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDesignerFilter.PreFilterEvents (IDictionary events)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDesignerFilter.PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object IRootDesigner.GetView (ViewTechnology viewTechnology)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			Dispose (true);
		}
	}
}

#endif
