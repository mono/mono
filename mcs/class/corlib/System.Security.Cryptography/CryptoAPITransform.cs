//
// System.Security.Cryptography CryptoAPITransform.cs
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (spouliot@motus.com)
//

using System;
using System.IO;

namespace System.Security.Cryptography {

public sealed class CryptoAPITransform : ICryptoTransform {

	private bool m_disposed = false;

	~CryptoAPITransform () 
	{
		Dispose (false);
	}

	[MonoTODO]
	public bool CanReuseTransform {
		// TODO: implement
		get { return true; }
	}

	/// <summary>
	/// Indicates if the Transform object can transform multiple blocks
	/// </summary>
	[MonoTODO]
	public bool CanTransformMultipleBlocks {
		// FIXME: should not be always true
		get { return true; }
	}

	[MonoTODO]
	public int InputBlockSize {
		// TODO: implement
		get { return 0;	}
	}

	[MonoTODO]
	public IntPtr KeyHandle {
		// TODO: implement
		get { return IntPtr.Zero; }
	}

	[MonoTODO]
	public int OutputBlockSize {
		// TODO: implement
		get { return 0; }
	}

	public void Dispose() 
	{
		Dispose (true);
		GC.SuppressFinalize (this);  // Finalization is now unnecessary
	}

	public void Clear() 
	{
		Dispose (false);
	}

	private void Dispose (bool disposing) 
	{
		if (!m_disposed) {
			// dispose unmanaged objects
			if (disposing) {
				// dispose managed objects
			}
			m_disposed = true;
		}
	}

	[MonoTODO]
	public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
	{
		// TODO: implement
		return 0;
	}

	[MonoTODO]
	public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
	{
		// TODO: implement
		return null;
	}
	
} // CryptoAPITransform
	
} // System.Security.Cryptography
