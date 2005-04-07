// Installer.cs
//   System.Configuration.Install.Installer class implementation
//
// Author:
//    Muthu Kannan (t.manki@gmail.com)
//
// (C) 2005 Novell, Inc.  http://www.novell.com/
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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;

namespace System.Configuration.Install
{

[DefaultEvent ("AfterInstall") ]
#if (!NET_2_0)
// .NET 2.0 (Community Preview) no longer has this attribute
[Designer("Microsoft.VisualStudio.Configuration.InstallerDesigner, " + Consts.AssemblyMicrosoft_VisualStudio, typeof(IRootDesigner))]
#endif
public class Installer : Component {
	private const string LAST_INSTALLER = "__LastInstaller";
	private const string STATE_PREFIX = "__State-";

	InstallContext context;
	InstallerCollection children;
	Installer parent;

	// Events
	public event InstallEventHandler BeforeInstall;
	public event InstallEventHandler AfterInstall;
	public event InstallEventHandler Committing;
	public event InstallEventHandler Committed;
	public event InstallEventHandler BeforeRollback;
	public event InstallEventHandler AfterRollback;
	public event InstallEventHandler BeforeUninstall;
	public event InstallEventHandler AfterUninstall;

	// Constructors
	public Installer ()
	{
		children = new InstallerCollection (this);
		Parent = null;
		Context = new InstallContext ();
	}

	// Properties
	[Browsable (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
			return "Installer";
		}
	}

	[Browsable (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public InstallerCollection Installers {
		get {
			return children;
		}
	}

	[Browsable (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[TypeConverter ("System.Configuration.Design.InstallerParentConverter")]
	public Installer Parent {
		get {
			return parent;
		}
		set {
			if (Parent == value)
				return;	// do nothing
			if (isInTree (value))
				throw new InvalidOperationException (
						"This operation will cause recursive parent/child relationship");
			parent = value;
		}
	}

	private bool isInTree (Installer ins)
	{
		if (ins == this)
			return true;
		foreach (Installer i in Installers)
			if (i == ins)
				return true;
			else
				return i.isInTree (ins);
		return false;
	}

	// Methods
	public virtual void Install (IDictionary state)
	{
		if (state == null)
			throw new ArgumentException ("State saver IDictionary cannot be null");

		try {
			OnBeforeInstall (state);
		} catch (Exception e) {
			Context.addToLog (e);
			throw new Exception ("An error occurred while processing BeforeInstall event", e);
		}

		int maxi = Installers.Count - 1;
		int i = 0;
		try {
			for (i = 0; i <= maxi; ++i) {
				IDictionary ht = new Hashtable ();
				try {
					Installers [i].Install (ht);
				} finally {
					state.Add (STATE_PREFIX + i.ToString(), ht);
				}
			}
		} finally {
			state.Add (LAST_INSTALLER, i > maxi ? maxi : i);
		}

		try {
			OnAfterInstall (state);
		} catch (Exception e) {
			Context.addToLog (e);
			throw new Exception ("An error occurred while processing AfterInstall event", e);
		}
	}

	public virtual void Commit (IDictionary state)
	{
		if (state == null)
			throw new ArgumentException ("State saver IDictionary cannot be null");
		if (state [LAST_INSTALLER] == null)
			throw new ArgumentException ("Invalid or corrupt state saver");
		int lastInstaller = (int)state [LAST_INSTALLER];
		IDictionary [] states = new IDictionary [lastInstaller + 1];
		for (int i = 0; i <= lastInstaller; ++i)
			if ((states [i] = (IDictionary)state [STATE_PREFIX + i.ToString()]) == null)
				throw new ArgumentException ("Invalid or corrupt state saver");

		Exception caught = null;
		try {
			OnCommitting (state);
		} catch (Exception e) {
			Context.addToLog (e);
			caught = e;
		}

		for (int i = 0; i <= lastInstaller; ++i) {
			try {
				Installers [i].Commit (states [i]);
			} catch (Exception e) {
				caught = e;
				Context.addToLog (e);
			}
		}

		try {
			OnCommitted (state);
		} catch (Exception e) {
			Context.addToLog (e);
			caught = e;
		}

		if (caught != null)
			throw new InstallException ("An error occurred while committing installation", caught);
	}

	public virtual void Rollback (IDictionary state)
	{
		if (state == null)
			throw new ArgumentException ("State saver IDictionary cannot be null");
		if (state [LAST_INSTALLER] == null)
			throw new ArgumentException ("Invalid or corrupt state saver");
		int lastInstaller = (int)state [LAST_INSTALLER];
		IDictionary [] states = new IDictionary [lastInstaller + 1];
		for (int i = 0; i <= lastInstaller; ++i)
			if ((states [i] = (IDictionary)state [STATE_PREFIX + i.ToString()]) == null)
				throw new ArgumentException ("Invalid or corrupt state saver");

		Exception caught = null;
		try {
			OnBeforeRollback (state);
		} catch (Exception e) {
			Context.addToLog (e);
			caught = e;
		}

		for (int i = 0; i <= lastInstaller; ++i) {
			try {
				Installers [i].Rollback (states [i]);
			} catch (Exception e) {
				caught = e;
				Context.addToLog (e);
			}
		}

		try {
			OnAfterRollback (state);
		} catch (Exception e) {
			Context.addToLog (e);
			caught = e;
		}

		if (caught != null)
			throw new InstallException ("An error occurred while rolling back installation", caught);
	}

	public virtual void Uninstall (IDictionary state)
	{
		int lastInstaller = -1;
		IDictionary [] states;
		if (state == null) {
			lastInstaller = Installers.Count - 1;
			states = new IDictionary [lastInstaller + 1];
			for (int i = 0; i <= lastInstaller; ++i)
				states [i] = null;
		} else {
			if (state [LAST_INSTALLER] == null)
				throw new ArgumentException ("Corrupt state saver");
			lastInstaller = (int)state [LAST_INSTALLER];
			if (lastInstaller != Installers.Count-1)
				throw new ArgumentException ("Corrupt state saver");
			states = new IDictionary [lastInstaller + 1];
			for (int i = 0; i <= lastInstaller; ++i)
				if ((states [i] = (IDictionary)state [STATE_PREFIX + i.ToString()]) == null)
					throw new ArgumentException ("Corrupt state saver");
		}

		Exception caught = null;
		try {
			OnBeforeUninstall (state);
		} catch (Exception e) {
			Context.addToLog (e);
			caught = e;
		}

		for (int i = 0; i <= lastInstaller; ++i) {
			try {
				Installers [i].Uninstall (states [i]);
			} catch (Exception e) {
				caught = e;
				Context.addToLog (e);
			}
		}

		try {
			OnAfterUninstall (state);
		} catch (Exception e) {
			Context.addToLog (e);
			caught = e;
		}

		if (caught != null)
			throw new InstallException ("An error occurred during uninstallation", caught);
	}

	protected virtual void OnBeforeInstall (IDictionary state)
	{
		if (BeforeInstall != null)
			BeforeInstall (this, new InstallEventArgs (state));
	}

	protected virtual void OnAfterInstall (IDictionary state)
	{
		if (AfterInstall != null)
			AfterInstall (this, new InstallEventArgs (state));
	}

	protected virtual void OnCommitting (IDictionary state)
	{
		if (Committing != null)
			Committing (this, new InstallEventArgs (state));
	}

	protected virtual void OnCommitted (IDictionary state)
	{
		if (Committed != null)
			Committed (this, new InstallEventArgs (state));
	}

	protected virtual void OnBeforeRollback (IDictionary state)
	{
		if (BeforeRollback != null)
			BeforeRollback (this, new InstallEventArgs (state));
	}

	protected virtual void OnAfterRollback (IDictionary state)
	{
		if (AfterRollback != null)
			AfterRollback (this, new InstallEventArgs (state));
	}

	protected virtual void OnBeforeUninstall (IDictionary state)
	{
		if (BeforeUninstall != null)
			BeforeUninstall (this, new InstallEventArgs (state));
	}

	protected virtual void OnAfterUninstall (IDictionary state)
	{
		if (AfterUninstall != null)
			AfterUninstall (this, new InstallEventArgs (state));
	}
}

}
