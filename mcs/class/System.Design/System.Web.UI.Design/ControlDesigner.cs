
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
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace System.Web.UI.Design
{
	[MonoTODO]
	public class ControlDesigner : HtmlControlDesigner
	{
		public ControlDesigner () {  }
		
		[MonoTODO]
		protected string CreatePlaceHolderDesignTimeHtml () { throw new NotImplementedException (); }
		[MonoTODO]
		protected string CreatePlaceHolderDesignTimeHtml (string instruction) { throw new NotImplementedException (); }
		[MonoTODO]
		public virtual string GetDesignTimeHtml () { throw new NotImplementedException (); }
#if NET_2_0
		[MonoNotSupported ("")]
		public static DesignTimeResourceProviderFactory GetDesignTimeResourceProviderFactory (IServiceProvider serviceProvider)
		{ throw new NotImplementedException (); }

		[MonoNotSupported ("")]
		public static ViewRendering GetViewRendering (Control control)
		{ throw new NotImplementedException (); }

		[MonoNotSupported ("")]
		public static ViewRendering GetViewRendering (ControlDesigner designer)
		{ throw new NotImplementedException (); }

		[MonoNotSupported ("")]
		protected string CreateErrorDesignTimeHtml (string errorMessage)
		{ throw new NotImplementedException (); }

		[MonoNotSupported ("")]
		protected string CreateErrorDesignTimeHtml (string errorMessage, Exception e)
		{ throw new NotImplementedException (); }

		[MonoNotSupported ("")]
		protected virtual Control CreateViewControl ()
		{ throw new NotImplementedException (); }

		[MonoNotSupported ("")]
		public Rectangle GetBounds ()
		{ throw new NotImplementedException (); }

		[MonoNotSupported ("")]
		public virtual string GetDesignTimeHtml (DesignerRegionCollection regions)
		{ throw new NotImplementedException (); }
		
		[MonoNotSupported ("")]
		public virtual string GetEditableDesignerRegionContent (EditableDesignerRegion region) { throw new NotImplementedException (); }
		
		[MonoNotSupported ("")]
		public virtual void SetEditableDesignerRegionContent (EditableDesignerRegion region, string content) { throw new NotImplementedException (); }

		[MonoNotSupported ("")]
		public virtual string GetPersistenceContent ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public ViewRendering GetViewRendering ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Invalidate ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Invalidate (Rectangle rectangle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void InvokeTransactedChange (IComponent component, TransactedChangeCallback callback, object context, string description)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void InvokeTransactedChange (IComponent component, TransactedChangeCallback callback, object context, string description, MemberDescriptor member)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void InvokeTransactedChange (IServiceProvider serviceProvider, IComponent component, TransactedChangeCallback callback, object context, string description, MemberDescriptor member)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Localize (IDesignTimeResourceWriter resourceWriter)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnAutoFormatApplied (DesignerAutoFormat appliedAutoFormat)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnComponentChanging (object sender, ComponentChangingEventArgs ce)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnPaint (PaintEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RegisterClone (object original, object clone)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SetRegionContent (EditableDesignerRegion region, string content)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		protected virtual string GetEmptyDesignTimeHtml () { throw new NotImplementedException (); }

		[MonoTODO]
		protected virtual string GetErrorDesignTimeHtml (Exception e) { throw new NotImplementedException (); }

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use GetPersistenceContent() instead")]
#endif
		public virtual string GetPersistInnerHtml () { throw new NotImplementedException (); }

		[MonoTODO]
		public override void Initialize (IComponent component) { throw new NotImplementedException (); }

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use DataBindings.Contains(string) instead")]
#endif
		public bool IsPropertyBound (string propName) { throw new NotImplementedException (); }

#if !NET_2_0
		[MonoTODO]
		protected override void OnBehaviorAttached () { throw new NotImplementedException (); }
#endif

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use DataBindings.Changed event instead")]
#endif
		protected override void OnBindingsCollectionChanged (string propName) { throw new NotImplementedException (); }

#if NET_2_0
		[MonoTODO]
		protected virtual void OnClick (DesignerRegionMouseEventArgs e) { throw new NotImplementedException (); }
#endif

		[MonoTODO]
		public virtual void OnComponentChanged (object sender, ComponentChangedEventArgs ce) { throw new NotImplementedException (); }

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use OnComponentChanged() instead")]
#endif
		protected virtual void OnControlResize () { throw new NotImplementedException (); }

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties) { throw new NotImplementedException (); }

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use OnComponentChanged() instead")]
#endif
		public void RaiseResizeEvent () { throw new NotImplementedException (); }

		[MonoTODO]
		public virtual void UpdateDesignTimeHtml () { throw new NotImplementedException (); }

		[MonoTODO]
		public virtual bool AllowResize { get { throw new NotImplementedException (); } }
		[MonoTODO]
#if NET_2_0
		[Obsolete ("It is documented as not in use anymore", true)]
#endif
		protected object DesignTimeElementView { get { throw new NotImplementedException (); } }
		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use SetViewFlags(ViewFlags.DesignTimeHtmlRequiresLoadComplete, true)")]
#endif
		public virtual bool DesignTimeHtmlRequiresLoadComplete { get { throw new NotImplementedException (); } }
		[MonoTODO]
		public virtual string ID { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use Tag.SetDirty() and Tag.IsDirty instead.")]
#endif
		public bool IsDirty { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use ContainerControlDesigner and EditableDesignerRegion")]
#endif
		public bool ReadOnly { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }

#if NET_2_0
		[MonoNotSupported ("")]
		public override DesignerActionListCollection ActionLists {
			get { throw new NotImplementedException (); }
		}

		[MonoNotSupported ("")]
		public virtual DesignerAutoFormatCollection AutoFormats {
			get { throw new NotImplementedException (); }
		}

		[MonoNotSupported ("")]
		protected virtual bool DataBindingsEnabled {
			get { throw new NotImplementedException (); }
		}

		[MonoNotSupported ("")]
		protected ControlDesignerState DesignerState {
			[MonoNotSupported ("")]
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		protected internal virtual bool HidePropertiesInTemplateMode {
			get { throw new NotImplementedException (); }
		}

		[MonoNotSupported ("")]
		public bool InTemplateMode {
			get { throw new NotImplementedException (); }
		}

		[MonoNotSupported ("")]
		protected WebFormsRootDesigner RootDesigner {
			get { throw new NotImplementedException (); }
		}

		[MonoNotSupported ("")]
		protected IControlDesignerTag Tag {
			get { throw new NotImplementedException (); }
		}

		[MonoNotSupported ("")]
		protected void SetViewFlags (ViewFlags viewFlags, bool setFlag)
		{
			throw new NotImplementedException ();
		}
		
		[MonoNotSupported ("")]
		public virtual TemplateGroupCollection TemplateGroups {
			[MonoNotSupported ("")]
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public Control ViewControl {
			[MonoNotSupported ("")]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoNotSupported ("")]
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public virtual bool ViewControlCreated {
			[MonoNotSupported ("")]
			get {
				throw new NotImplementedException ();
			}

			[MonoNotSupported ("")]
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoNotSupported ("")]
		protected virtual bool UsePreviewControl {
			get {
				throw new NotImplementedException ();
			}
		}
#endif
	}
}
