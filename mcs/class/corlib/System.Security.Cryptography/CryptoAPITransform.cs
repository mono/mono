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

// Note: This class isn't used by Mono as all algorithms are provided with
// 100% managed implementations.

public sealed class CryptoAPITransform : ICryptoTransform {

	private bool m_disposed;

	internal CryptoAPITransform () 
	{
		m_disposed = false;
	}

	~CryptoAPITransform () 
	{
		Dispose (false);
	}

	public bool CanReuseTransform {
		get { return true; }
	}

	public bool CanTransformMultipleBlocks {
		get { return true; }
	}

	public int InputBlockSize {
		get { return 0;	}
	}

	public IntPtr KeyHandle {
		get { return IntPtr.Zero; }
	}

	public int OutputBlockSize {
		get { return 0; }
	}

	void IDisposable.Dispose () 
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

	public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
	{
		return 0;
	}

	public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
	{
		return null;
	}
	
} // CryptoAPITransform
	
} // System.Security.Cryptography
