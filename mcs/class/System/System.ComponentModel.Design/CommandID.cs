// System.ComponentModel.Design.CommandID.cs
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
	public class CommandID
	{
	
		[MonoTODO]
		internal CommandID (string text) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public CommandID (Guid menuGroup, int commandID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Guid Guid 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual int ID 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
	}
}
