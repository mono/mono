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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Alan McGovern (amcgovern@novell.com)
//

using System;
using System.Collections.Generic;

namespace System.IO.Packaging.Tests {
    
    class FakePackagePart : PackagePart {

        public List<FileAccess> Accesses { get; private set; }
        public List<FileMode> Modes { get; private set; }

        public FakePackagePart (Package package, Uri partUri)
            : base (package, partUri)
        {
            Init ();
        }

        public FakePackagePart (Package package, Uri partUri, string contentType)
            : base(package, partUri, contentType)
        {
            Init ();
        }

        public FakePackagePart (Package package, Uri partUri, string contentType, CompressionOption compressionOption)
            : base (package, partUri, contentType, compressionOption)
        {
            Init ();
        }

        private void Init ()
        {
            Accesses = new List<FileAccess> ();
            Modes = new List<FileMode> ();
        }

        protected override Stream GetStreamCore (FileMode mode, FileAccess access)
        {
            Accesses.Add (access);
            Modes.Add (mode);
            return new MemoryStream ();
        }
    }
}
