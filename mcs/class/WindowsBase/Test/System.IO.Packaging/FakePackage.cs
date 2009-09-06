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
	
    public class FakePackage : Package {
        Dictionary<Uri, PackagePart> Parts { get; set; }
        public List<Uri> CreatedParts { get; private set; }
        public List<Uri> DeletedParts { get; private set; }
        public List<Uri> GotParts { get; private set; }

        public FakePackage (FileAccess access, bool streaming)
            : base (access, streaming)
        {
            CreatedParts = new List<Uri> ();
            DeletedParts = new List<Uri> ();
            GotParts = new List<Uri>();
            Parts = new Dictionary<Uri, PackagePart> ();
        }

        protected override PackagePart CreatePartCore (Uri partUri, string contentType, CompressionOption compressionOption)
        {
            FakePackagePart p = new FakePackagePart (this, partUri, contentType, compressionOption);
            Parts.Add (p.Uri, p);
            CreatedParts.Add (partUri);
            return p;
        }

        protected override void DeletePartCore (Uri partUri)
        {
            DeletedParts.Add (partUri);
            Parts.Remove (partUri);
        }

        protected override void FlushCore ()
        {
            // Flush...
        }
        
        protected override PackagePart GetPartCore (Uri partUri)
        {
            if (!GotParts.Contains (partUri))
                GotParts.Add (partUri);
            return Parts.ContainsKey(partUri) ?  Parts [partUri] : null;
        }

        protected override PackagePart [] GetPartsCore ()
        {
            PackagePart [] p = new PackagePart [Parts.Count];
            Parts.Values.CopyTo (p, 0);
            return p;
        }
    }
}