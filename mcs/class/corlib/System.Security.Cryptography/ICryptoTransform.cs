//
// System.Security.Cryptography ICryptoTransform interface
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
//

using System;

namespace System.Security.Cryptography {

/// <summary>
/// Crytographic functions that can process a stream of bytes implement this interface.
/// This works by stringing together one or more ICryptoTransform classes with a stream.
/// Data is passed from one to the next without the need of outside buffering/intervention.
/// </summary>
public interface ICryptoTransform : IDisposable {

	bool CanReuseTransform {get;}

	/// <summary>
	/// Whether the function can transform multiple blocks at a time.
	/// </summary>
	bool CanTransformMultipleBlocks {get;}

	/// <summary>
	/// Size of input blocks for the function in bytes.
	/// </summary>
	int InputBlockSize {get;}

	/// <summary>
	/// Size of the output blocks of the function in bytes.
	/// </summary>
	int OutputBlockSize {get;}

	/// <summary>
	/// FIXME: Process some data.  A block at a time?  Less than a block at a time?
	/// </summary>
	int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

	/// <summary>
	/// Processes the final part of the data.  Also finalizes the function if needed.
	/// </summary>
	byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount);
}

}

