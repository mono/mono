/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// 
// CompressionMOode.cs
//
// Authors:
//	Christopher James Lahey <clahey@ximian.com>
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

namespace System.IO.Compression {
	public enum CompressionMode {
		Decompress=0,	// Decompress the given stream.
		Compress=1	// Compress the given stream.
	};
}

