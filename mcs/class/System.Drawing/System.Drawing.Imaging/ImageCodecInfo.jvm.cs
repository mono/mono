//
// System.Drawing.Imaging.ImageCodecInfo.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//   Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using Mainsoft.Drawing.Imaging;

using imageio = javax.imageio;
using spi = javax.imageio.spi;

namespace System.Drawing.Imaging {

	[ComVisible (false)]
	public sealed class ImageCodecInfo
	{
		private Guid clsid;
		private string codecName;
		private string dllName;
		private string filenameExtension;
		private ImageCodecFlags flags;
		private string formatDescription;
		private Guid formatID;
		private string	mimeType;
		private byte[][] signatureMasks;
		private byte[][] signaturePatterns;
		private int version;
		
		public static ImageCodecInfo[] GetImageDecoders () 
		{
			Hashtable oldInfo = ImageCodec.Decoders;
			ImageCodecInfo [] newInfo = new ImageCodecInfo [oldInfo.Count];
			int i=0;
			foreach (ImageCodecInfo codec in oldInfo.Values) {
				newInfo [i++] = (ImageCodecInfo) codec.MemberwiseClone ();
			}
			return newInfo;
		}
		
		internal ImageCodecInfo () {
		}

		public static ImageCodecInfo[] GetImageEncoders () 
		{
			Hashtable oldInfo = ImageCodec.Encoders;
			ImageCodecInfo [] newInfo = new ImageCodecInfo [oldInfo.Count];
			int i=0;
			foreach (ImageCodecInfo codec in oldInfo.Values) {
				//newInfo [i++] = (ImageCodecInfo) codec.MemberwiseClone ();
				newInfo [i] = new ImageCodecInfo ();
				newInfo [i].clsid = codec.clsid;
				newInfo [i].formatID = codec.formatID;
				newInfo [i].codecName = codec.codecName;
				newInfo [i].dllName = codec.dllName;
				newInfo [i].flags = codec.flags;
				newInfo [i].filenameExtension = codec.filenameExtension;
				newInfo [i].formatDescription = codec.formatDescription;
				newInfo [i].mimeType = codec.mimeType;
				newInfo [i].signatureMasks = codec.signatureMasks;
				newInfo [i].signaturePatterns = codec.signaturePatterns;
				newInfo [i++].version = codec.version;
			}
			return newInfo;
		}

		// properties
		
		public Guid Clsid 
		{
			get { return clsid; }
			set { clsid = value; }
		}

		
		public string CodecName 
		{
			get { return codecName; }
			set { codecName = value; }
		}

		
		public string DllName 
		{
			get { return dllName; }
			set { throw new PlatformNotSupportedException(); }
		}

		
		public string FilenameExtension 
		{
			get { return filenameExtension; }
			set { filenameExtension = value; }
		}

		
		public ImageCodecFlags Flags 
		{
			get { return flags; }
			set { flags = value; }
		}
		
		public string FormatDescription 
		{
			get { return formatDescription; }
			set { formatDescription = value; }
		}
		
		public Guid FormatID 
		{
			get { return formatID; }
			set { formatID = value; }
		}

		
		public string MimeType 
		{
			get { return mimeType; }
			set { mimeType = value; }
		}

		
		[CLSCompliant(false)]
		public byte[][] SignatureMasks 
		{
			get { return signatureMasks; }
			set { signatureMasks = value; }
		}

		[CLSCompliant(false)]
		public byte[][] SignaturePatterns 
		{
			get { return signaturePatterns; }
			set { signaturePatterns = value; }
		}
		
		public int Version 
		{
			get { return version; }
			set { version = value; }
		}

	}

}
