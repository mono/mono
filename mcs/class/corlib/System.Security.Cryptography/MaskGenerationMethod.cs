//
// MaskGenerationMethod.cs: Handles mask generation.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace System.Security.Cryptography
{

public abstract class MaskGenerationMethod
{
	protected MaskGenerationMethod () {}

	public abstract byte[] GenerateMask (byte[] rgbSeed, int cbReturn);
}

}