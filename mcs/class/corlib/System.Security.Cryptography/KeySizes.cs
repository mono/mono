//
// System.Security.Cryptography KeySizes Class implementation
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Ben Maurer (bmaurer@users.sf.net)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright 2001 by Matthew S. Ford.
// (C) 2004 Novell (http://www.novell.com)
//

namespace System.Security.Cryptography {
	
#if NET_1_0
	public class KeySizes {
#else
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
