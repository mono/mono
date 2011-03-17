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
		private Hashtable properties;

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

		public virtual IDictionary Properties {
			get {
				if (properties == null)
					properties = new Hashtable ();
				return properties;
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
		
		public virtual void Invoke (object arg)
		{
			this.Invoke ();
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
