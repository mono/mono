//
// MetadataReference.cs: Represents a metadata reference in expression.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class MetadataReference {
	
		string		itemName;
		string		metadataName;
		OldExpression	parent;
	
		public MetadataReference (OldExpression parent)
		{
			this.itemName = null;
			this.metadataName = null;
			this.parent = parent;
		}

		public MetadataReference (OldExpression parent, string itemName, string metadataName)
		{
			this.parent = parent;
			this.itemName = itemName;
			this.metadataName = metadataName;
		}
		
		public void ParseSource (string source)
		{
			string sourceWithoutParens;
			int dot;
		
			if (source.Length < 4)
				throw new ArgumentException ("source is too short.");
			
			sourceWithoutParens = source.Substring (2, source.Length - 3);
			dot = sourceWithoutParens.IndexOf ('.');
			
			if (dot != -1) {
				itemName = sourceWithoutParens.Substring (0, dot);
				metadataName = sourceWithoutParens.Substring (dot + 1, sourceWithoutParens.Length - dot - 1); 
			} else {
				metadataName = sourceWithoutParens;
			}
		}
		
		public string ItemName {
			get { return itemName; }
		}
		
		public string MetadataName {
			get { return metadataName; }
		}
		
		public bool IsQualified {
			get { return (itemName == null) ? false : true; }
		}
	}
}

#endif