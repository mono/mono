// System.Configuration.Install.Installer.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// Alejandro Sánchez Acosta
// 

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Configuration.Install
{
	[DefaultEvent("AfterInstall")]
#if (!NET_2_0)
	// .NET 2.0 (Community Preview) no longer has this attribute
	[Designer("Microsoft.VisualStudio.Configuration.InstallerDesigner, " + Consts.AssemblyMicrosoft_VisualStudio, typeof(IRootDesigner))]
#endif
	public class Installer : Component
	{
		private InstallContext context;
		private string helptext;
		private InstallerCollection installers;
		internal Installer parent;
		
		[MonoTODO]
		public Installer () {
			throw new NotImplementedException ();
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute(false)]
		public InstallContext Context {
			get {
				return context;
			}
			
			set {
				context = value;
			}
		}

		public virtual string HelpText {
			get {
				return helptext;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[BrowsableAttribute(false)]
		public InstallerCollection Installers {
			get {
				return installers;
			}
		}

		[TypeConverter ("System.Configuration.Design.InstallerParentConverter")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public Installer Parent {
			get {
				return parent;
			}
			
			set {
				parent = value;
			}
		}
		
		[MonoTODO]
		public virtual void Commit (IDictionary savedState) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Install (IDictionary stateSaver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnAfterInstall (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnAfterRollback (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnAfterUninstall (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnBeforeInstall (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnBeforeRollback (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnBeforeUninstall (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnCommitted (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnCommitting (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void Rollback (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Uninstall (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}
		
		public event InstallEventHandler AfterInstall;

		public event InstallEventHandler AfterRollback;

		public event InstallEventHandler AfterUninstall;

		public event InstallEventHandler BeforeInstall;

		public event InstallEventHandler BeforeRollback;
		
		public event InstallEventHandler BeforeUninstall;

		public event InstallEventHandler Committed;

		public event InstallEventHandler Committing;		
	}
}
