//
// System.ComponentModel.Design.CommandID.cs
//
// Author:
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
	public class CommandID
	{
		private int cID;
		private Guid guid;

		public CommandID (Guid menuGroup, int commandID)
		{
			cID = commandID;
			guid = menuGroup;
		}

		public virtual Guid Guid {
			get {
				return guid;
			}
		}

		public virtual int ID {
			get {
				return cID;
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is CommandID))
				return false;
			if (obj == this)
				return true;
			return ((CommandID) obj).Guid.Equals (guid) && 
				((CommandID) obj).ID.Equals (cID);
		}

		public override int GetHashCode() 
		{
			// Guid can only be valid
			return guid.GetHashCode() ^ cID.GetHashCode();
		}

		public override string ToString()
		{
			return guid.ToString () + " : " + cID.ToString ();
		}
	}
}
