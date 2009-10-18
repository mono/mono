//
// System.Resources.InternalResourceSet
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using System.IO;

namespace System.Resources {

	// ResourceSet.GetObject always returns the same object reference - but ResourceManager doesn't use it!
	// Instead another internal type, RuntimeResourceSet, is used to return a cloned object (fix for bug #366489)
	[Serializable]
	internal class RuntimeResourceSet : ResourceSet {

		// Constructor for Activator.CreateInstance from Silverlight
		public RuntimeResourceSet (UnmanagedMemoryStream stream) : base (stream)
		{
		}
		
		public RuntimeResourceSet (Stream stream) :
			base (stream)
		{
		}

		public RuntimeResourceSet (string fileName) :
			base (fileName)
		{
		}

		public override object GetObject (string name)
		{
			if (Reader == null)
#if NET_2_0
				throw new ObjectDisposedException ("ResourceSet is closed.");
#else
				throw new InvalidOperationException ("ResourceSet is closed.");
#endif

			return CloneDisposableObjectIfPossible (base.GetObject (name));
		}

		public override object GetObject (string name, bool ignoreCase)
		{
			if (Reader == null)
#if NET_2_0
				throw new ObjectDisposedException ("ResourceSet is closed.");
#else
				throw new InvalidOperationException ("ResourceSet is closed.");
#endif

			return CloneDisposableObjectIfPossible (base.GetObject (name, ignoreCase));
		}

		// if possible return a clone of the object if it's (a) clonable and (b) disposable
		private object CloneDisposableObjectIfPossible (object value)
		{
			ICloneable clonable = (value as ICloneable);
			return (clonable != null && (value is IDisposable)) ? clonable.Clone () : value;
		}
	}
}
