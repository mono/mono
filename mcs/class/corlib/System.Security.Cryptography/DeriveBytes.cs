//
// System.Security.Cryptography DeriveBytes.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;

namespace System.Security.Cryptography {

	public abstract class DeriveBytes {
	
		protected DeriveBytes ()
		{
		}
		
		public abstract byte[] GetBytes (int cb);

		public abstract void Reset ();
	}
}
