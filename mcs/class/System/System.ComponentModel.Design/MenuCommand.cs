// System.ComponentModel.Design.MenuCommand.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	public class MenuCommand
	{
		[MonoTODO]
		public MenuCommand (EventHandler handler, CommandID command) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool Checked 
		{
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual CommandID CommandID 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual bool Enabled 
		{
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual int OleStatus 
		{
			get {
				throw new NotImplementedException ();
			}		

			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual bool Supported 
		{
			get {
				throw new NotImplementedException (); 
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual bool Visible 
		{
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual void Invoke()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnCommandChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
		
		public event EventHandler CommandChanged;
	}	
}
