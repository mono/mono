//
// System.ComponentModel.Design.MenuCommand.cs
//
// Authors:
//   Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// 

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	public class MenuCommand
	{

		private EventHandler handler;
		private CommandID command;
		private bool ischecked = false;
		private bool enabled = true;
		private bool issupported = true;
		private bool visible = true;

		public MenuCommand (EventHandler handler, CommandID command)
		{
			this.handler = handler;
			this.command = command;
		}

		public virtual bool Checked {
			get {
				return ischecked;
			}
			set {
				if (ischecked != value) {
					ischecked = value;
					OnCommandChanged (EventArgs.Empty);
				}
			}
		}

		public virtual CommandID CommandID {
			get {
				return command;
			}
		}

		public virtual bool Enabled {
			get {
				return enabled;
			}
			set {
				if (enabled != value) 
				{
					enabled = value;
					OnCommandChanged (EventArgs.Empty);
				}
			}
		}

		[MonoTODO]
		public virtual int OleStatus {
			get {
				// This is calcualted from the other properties, but the docs to not tell how
				// Default seems to be "3", but changes with diffentent set properties
				return 3;
			}		
		}

		public virtual bool Supported {
			get {
				return issupported; 
			}
			
			set {
				issupported = value;
			}
		}

		public virtual bool Visible {
			get {
				return visible;
			}
			
			set {
				visible = value;
			}
		}

		public virtual void Invoke()
		{
			// FIXME Not sure if this invocation is what should be done here
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
		
		protected virtual void OnCommandChanged (EventArgs e)
		{
			if (CommandChanged != null)
				CommandChanged (this, e);
		}

		public override string ToString()
		{
			// MS runtime produces a NullReferenceException here if CommandID property == null
			// which I guess isn't a good idea (throwing exceptions in ToStrings???) - bug in MS??
			string commandpart = string.Empty;
			if (command != null)
				commandpart = command.ToString ();
			commandpart = string.Concat (commandpart, " : ");
			if (this.Supported)
				commandpart = string.Concat (commandpart, "Supported");
			if (this.Enabled)
  				commandpart = string.Concat (commandpart, "|Enabled");
			if (this.Visible)
				commandpart = string.Concat (commandpart, "|Visible");
			if (this.Checked)
				commandpart = string.Concat (commandpart, "|Checked");
			return commandpart;
		}
		
		public event EventHandler CommandChanged;
	}	
}
