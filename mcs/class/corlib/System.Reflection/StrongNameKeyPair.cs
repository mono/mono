//
// System.Reflection.StrongNameKeyPair.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

using System.IO;

namespace System.Reflection {

	[Serializable]
	public class StrongNameKeyPair 
	{		
		private byte[] keyPair;

		public StrongNameKeyPair (byte[] keyPairArray) 
		{
			keyPair = keyPairArray;
		}
		
		public StrongNameKeyPair (FileStream keyPairFile) 
		{
			
		}
		
		public StrongNameKeyPair (string keyPairContainer) 
		{
			
		}
		
		public byte[] PublicKey 
		{
			get { return keyPair; }
		}
	}
}
