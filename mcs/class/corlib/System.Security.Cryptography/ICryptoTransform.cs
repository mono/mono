//
// System.Security.Cryptography ICryptoTransform interface
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
// (C) 2004 Novell (http://www.novell.com)
//

using System;

namespace System.Security.Cryptography {

	public interface ICryptoTransform : IDisposable {

		bool CanReuseTransform {
			get;
		}

		bool CanTransformMultipleBlocks {
			get;
		}

		int InputBlockSize {
			get;
		}

		int OutputBlockSize {
			get;
		}

		int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

		byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount);
	}
}
