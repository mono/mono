//
// System.Security.Cryptography KeySizes Class implementation
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Ben Maurer (bmaurer@users.sf.net)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright 2001 by Matthew S. Ford.
//

namespace System.Security.Cryptography {
	
	/// <summary>
	/// This class represents valid ranges of key sizes for ciphers.  It is also used to represent block sizes in the same fashion for block ciphers.
	/// </summary>
#if USE_VERSION_1_0
	public class KeySizes {
#else
	public sealed class KeySizes {
#endif
		private int _maxSize;
		private int _minSize;
		private int _skipSize;

		/// <summary>
		/// Creates a new KeySizes object.
		/// </summary>
		/// <param name="minSize">The minimum size key allowed for this cipher in bits.</param>
		/// <param name="maxSize">The maximum size key allowed for this cipher in bits.</param>
		/// <param name="skipSize">The jump/skip between the valid key sizes in bits.</param>
		public KeySizes (int minSize, int maxSize, int skipSize) 
		{
			_maxSize = maxSize;
			_minSize = minSize;
			_skipSize = skipSize;
		}
		
		/// <summary>
		/// Returns the maximum valid key size in bits;
		/// </summary>
		public int MaxSize {
			get { return _maxSize; }
		}
		
		/// <summary>
		/// Returns the minimum valid key size in bits;
		/// </summary>
		public int MinSize {
			get { return _minSize; }
		}
		
		/// <summary>
		/// Returns the skip between valid key sizes in bits;
		/// </summary>
		public int SkipSize {
			get { return _skipSize; }
		}
	
		/// <summary>
		/// Checks if a key of keySize bits is valid, according to this
		/// rule.
		/// </summary>
		/// <param name="keySize">The keysize to check.</param>
		/// <returns>True if the key size is legal, else false.</returns>
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
