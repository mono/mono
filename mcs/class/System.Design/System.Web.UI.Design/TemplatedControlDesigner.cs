//
// System.Web.UI.Design.TemplatedControlDesigner
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System;
using System.Collections;
using System.ComponentModel.Design;

namespace System.Web.UI.Design
{
	public abstract class TemplatedControlDesigner : ControlDesigner
	{
		public TemplatedControlDesigner ()
		{
		}

		protected abstract ITemplateEditingFrame CreateTemplateEditingFrame (TemplateEditingVerb verb);
		protected abstract TemplateEditingVerb[] GetCachedTemplateEditingVerbs ();
		public abstract string GetTemplateContent (ITemplateEditingFrame editingFrame, string templateName, out bool allowEditing);
		public abstract void SetTemplateContent (ITemplateEditingFrame editingFrame, string templateName, string templateContent);

		[MonoTODO]
		public void EnterTemplateMode (ITemplateEditingFrame newTemplateEditingFrame)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ExitTemplateMode (bool fSwitchingTemplates, bool fNested, bool fSave)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetPersistInnerHtml ()
		{
			throw new NotImplementedException ();
		}

		public virtual string GetTemplateContainerDataItemProperty (string templateName)
		{
			return string.Empty;
		}

		public virtual IEnumerable GetTemplateContainerDataSource (string templateName)
		{
			return null;
		}

		[MonoTODO]
		public TemplateEditingVerb[] GetTemplateEditingVerbs ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected ITemplate GetTemplateFromText (string text)
		{
			throw new NotImplementedException ();
		}

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

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

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

		protected virtual bool HidePropertiesInTemplateMode {
			get {
				return true;
			}
		}

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
