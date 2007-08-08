//
// AssemblyHash.cs
//
// Authors:
//	Tomas Restrepo (tomasr@mvps.org)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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

namespace System.Configuration.Assemblies {

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
	[Obsolete]
#endif
	public struct AssemblyHash : ICloneable {

		private AssemblyHashAlgorithm _algorithm;
		private byte[] _value;

#if NET_2_0
		[Obsolete]
#endif
		public static readonly AssemblyHash Empty = new AssemblyHash (AssemblyHashAlgorithm.None, null);

#if NET_2_0
		[Obsolete]
#endif
		public AssemblyHashAlgorithm Algorithm {
			get { return _algorithm; }
			set { _algorithm = value; }
		}


#if NET_2_0
		[Obsolete]
#endif
		public AssemblyHash (AssemblyHashAlgorithm algorithm, byte[] value)
		{
			_algorithm = algorithm;
			if (value != null)
				_value = (byte[]) value.Clone ();
			else
				_value = null;
		}

#if NET_2_0
		[Obsolete]
#endif
		public AssemblyHash (byte[] value)
			: this (AssemblyHashAlgorithm.SHA1, value)
		{
		}

#if NET_2_0
		[Obsolete]
#endif
		public object Clone ()
		{
			return new AssemblyHash (_algorithm, _value);
		}

#if NET_2_0
		[Obsolete]
#endif
		public byte[] GetValue ()
		{
			return _value;
		}

#if NET_2_0
		[Obsolete]
#endif
		public void SetValue (byte[] value)
		{
			_value = value;
		}
	}
}
