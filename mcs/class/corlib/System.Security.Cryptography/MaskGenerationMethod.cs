//
// MaskGenerationMethod.cs: Handles mask generation.
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

namespace System.Security.Cryptography {

	public abstract class MaskGenerationMethod {

		protected MaskGenerationMethod () 
		{
		}

		public abstract byte[] GenerateMask (byte[] rgbSeed, int cbReturn);
	}
}
