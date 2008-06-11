// System.Configuration.Install.Installer.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// Alejandro Sánchez Acosta
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

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Configuration.Install
{
	[DefaultEvent("AfterInstall")]
	public class Installer : Component
	{
		private InstallContext context;
		private string helptext;
		private InstallerCollection installers;
		internal Installer parent;
		
		public Installer () {
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

		public InstallerCollection Installers {
			get {
				if (installers == null)
					installers = new InstallerCollection (this);
				return installers;
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
		
		public virtual void Commit (IDictionary savedState) 
		{
		}

		public virtual void Install (IDictionary stateSaver)
		{
		}

		protected virtual void OnAfterInstall (IDictionary savedState)
		{
			if (AfterInstall != null)
				AfterInstall (this, new InstallEventArgs (savedState));
		}
		
		protected virtual void OnAfterRollback (IDictionary savedState)
		{
			if (AfterRollback != null)
				AfterRollback (this, new InstallEventArgs (savedState));
		}

		protected virtual void OnAfterUninstall (IDictionary savedState)
		{
			if (AfterUninstall != null)
				AfterUninstall (this, new InstallEventArgs (savedState));
		}
		
		protected virtual void OnBeforeInstall (IDictionary savedState)
		{
			if (BeforeInstall != null)
				BeforeInstall (this, new InstallEventArgs (savedState));
		}
		
		protected virtual void OnBeforeRollback (IDictionary savedState)
		{
			if (BeforeRollback != null)
				BeforeRollback (this, new InstallEventArgs (savedState));
		}

		protected virtual void OnBeforeUninstall (IDictionary savedState)
		{
			if (BeforeUninstall != null)
				BeforeUninstall (this, new InstallEventArgs (savedState));
		}
		
		protected virtual void OnCommitted (IDictionary savedState)
		{
			if (Committed != null)
				Committed (this, new InstallEventArgs (savedState));
		}
		
		protected virtual void OnCommitting (IDictionary savedState)
		{
			if (Committing != null)
				Committing (this, new InstallEventArgs (savedState));
		}
		
		public virtual void Rollback (IDictionary savedState)
		{
		}

		public virtual void Uninstall (IDictionary savedState)
		{
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
