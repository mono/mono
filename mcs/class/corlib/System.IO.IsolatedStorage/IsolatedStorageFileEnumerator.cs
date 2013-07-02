//
// System.IO.IsolatedStorage.IsolatedStorageFileEnumerator
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;

namespace System.IO.IsolatedStorage {

	internal class IsolatedStorageFileEnumerator : IEnumerator {

		private IsolatedStorageScope _scope;
		private string[] _storages;
		private int _pos;

		public IsolatedStorageFileEnumerator (IsolatedStorageScope scope, string root)
		{
			_scope = scope;
			// skip application-isolated storages
			if (Directory.Exists (root))
				_storages = Directory.GetDirectories (root, "d.*");
			_pos = -1;
		}

		public object Current {
			get {
				if ((_pos < 0) || (_storages == null) || (_pos >= _storages.Length))
					return null;
				// recreates a IsolatedStorageFile from the file
				return new IsolatedStorageFile (_scope, _storages [_pos]);
			}
		}

		public bool MoveNext ()
		{
			if (_storages == null)
				return false;
			return (++_pos < _storages.Length);
		}

		public void Reset ()
		{
			_pos = -1;
		}
	}
}
