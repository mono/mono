// System.Configuration.Install.Installer.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// Alejandro Sánchez Acosta
// 

using System.Collections;
using System.ComponentModel;

namespace System.Configuration.Install
{
	public class Installer : Component
	{
		private InstallContext context;
		private string helptext;
		InstallerCollection installers;
		Installer parent;
		
		[MonoTODO]
		public Installer () {
			throw new NotImplementedException ();
		}

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
			
			set {
				helptext = value;
			}
		}

		public InstallerCollection Installers {
			get {
				return installers;
			}
			
			set {
				installers = value;		
			}
		}

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
