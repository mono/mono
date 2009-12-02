//
// System.Security.Cryptography KeySizes Class implementation
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Ben Maurer (bmaurer@users.sf.net)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright 2001 by Matthew S. Ford.
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Security.Cryptography {
	
#if NET_1_0
	public class KeySizes {
#else
	#if NET_2_0 && !NET_2_1
	[ComVisible (true)]
	#endif
	public sealed class KeySizes {
#endif
		private int _maxSize;
		private int _minSize;
		private int _skipSize;

		public KeySizes (int minSize, int maxSize, int skipSize) 
		{
			_maxSize = maxSize;
			_minSize = minSize;
			_skipSize = skipSize;
		}
		
		public int MaxSize {
			get { return _maxSize; }
		}
		
		public int MinSize {
			get { return _minSize; }
		}
		
		public int SkipSize {
			get { return _skipSize; }
		}
	
		internal bool IsLegal (int keySize) 
		{
			int ks = keySize - MinSize;
			bool result = ((ks >= 0) && (keySize <= MaxSize));
			return ((SkipSize == 0) ? result : (result && (ks % SkipSize == 0)));
		}

		internal static bool IsLegalKeySize (KeySizes[] legalKeys, int size) 
		{
			foreach (KeySizes legalKeySize in legalKeys) {
				if (legalKeySize.IsLegal (size)) 
					return true;
			}
			return false;
		}
	}
}
