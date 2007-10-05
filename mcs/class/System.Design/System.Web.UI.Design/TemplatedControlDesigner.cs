//
// System.Web.UI.Design.TemplatedControlDesigner
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
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

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.Design
{
	public abstract class TemplatedControlDesigner : ControlDesigner
	{
		public TemplatedControlDesigner ()
		{
		}

#if NET_2_0
		[MonoTODO]
		public override void Initialize (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		protected abstract ITemplateEditingFrame CreateTemplateEditingFrame (TemplateEditingVerb verb);

#if NET_2_0
		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		protected abstract TemplateEditingVerb[] GetCachedTemplateEditingVerbs ();

#if NET_2_0
		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		public abstract string GetTemplateContent (ITemplateEditingFrame editingFrame, string templateName, out bool allowEditing);

#if NET_2_0
		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		public abstract void SetTemplateContent (ITemplateEditingFrame editingFrame, string templateName, string templateContent);

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		public void EnterTemplateMode (ITemplateEditingFrame newTemplateEditingFrame)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		public void ExitTemplateMode (bool fSwitchingTemplates, bool fNested, bool fSave)
		{
			throw new NotImplementedException ();
		}

#if !NET_2_0
		[MonoTODO]
		public override string GetPersistInnerHtml ()
		{
			throw new NotImplementedException ();
		}
#endif

#if NET_2_0
		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		public virtual string GetTemplateContainerDataItemProperty (string templateName)
		{
			return string.Empty;
		}

#if NET_2_0
		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		public virtual IEnumerable GetTemplateContainerDataSource (string templateName)
		{
			return null;
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		public TemplateEditingVerb[] GetTemplateEditingVerbs ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected ITemplate GetTemplateFromText (string text)
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		public virtual Type GetTemplatePropertyParentType (string templateName)
		{
			return base.Component.GetType ();
		}

		[MonoTODO]
		protected string GetTextFromTemplate (ITemplate template)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use ControlDesigner.Tag instead")]
#endif
		protected override void OnBehaviorAttached ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void OnComponentChanged (object sender, ComponentChangedEventArgs ce)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void OnSetParent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnTemplateModeChanged ()
		{
			throw new NotImplementedException ();
		}

#if !NET_2_0
		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		protected void SaveActiveTemplateEditingFrame ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void UpdateDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		protected override bool DataBindingsEnabled {
			get { throw new NotImplementedException (); }
		}

		public override TemplateGroupCollection TemplateGroups {
			get { throw new NotImplementedException (); }
		}
#endif

#if NET_2_0
		[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
		public ITemplateEditingFrame ActiveTemplateEditingFrame {
			get {
				return _activeTemplateFrame;
			}
		}

		public bool CanEnterTemplateMode {
			get {
				return _enableTemplateEditing;
			}
		}

#if !NET_2_0
		protected internal virtual bool HidePropertiesInTemplateMode {
			get {
				return true;
			}
		}
#endif

#if NET_2_0
		[Obsolete ("Use ControlDesigner.InTemplateMode instead")]
#endif
		public bool InTemplateMode {
			get {
				return _templateMode;
			}
		}

		internal EventHandler TemplateEditingVerbHandler {
			get {
				return _templateVerbHandler;
			}
		}

		private ITemplateEditingFrame _activeTemplateFrame;
		private bool _enableTemplateEditing = true;
		private bool _templateMode;
		private EventHandler _templateVerbHandler;
	}
}
