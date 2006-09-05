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
