//
// ItemReference.cs: Represents "@(Reference)" in expression.
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
using System.Collections;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class ItemReference {
	
		string		itemName;
		string		separator;
		Expression	parent;
		Expression	transform;
	
		public ItemReference (Expression parent)
		{
			if (parent == null)
				throw new Exception ("Parent Expression needed to find project.");
			this.parent = parent;
			this.itemName = null;
			this.separator = ";";
		}
		
		public ItemReference (Expression parent, string source)
			: this (parent)
		{
			ParseSource (source);
		}
		
		public void ParseSource (string source)
		{
			string sourceWithoutParens;
			ApostropheState aState = ApostropheState.Out;
			ItemParsingState iState = ItemParsingState.Name;
			int c = -1;
			int itemNameEnd;
			int transformEnd = -1;
			int separatorStart = -1;
		
			if (source == null)
				throw new ArgumentNullException ("source");
		
			if (source.Length < 3)
				throw new ArgumentException ("Invalid item.");
			
			sourceWithoutParens = source.Substring (2, source.Length - 3);
			itemNameEnd = sourceWithoutParens.Length - 1;
			CharEnumerator it = sourceWithoutParens.GetEnumerator ();

			while (it.MoveNext ()) {
				c++;
				if (it.Current == '\'') {
					if (aState == ApostropheState.In)
						aState = ApostropheState.Out;
					else
						aState = ApostropheState.In;
				}
				if (it.Current == '-' && iState == ItemParsingState.Name && aState == ApostropheState.Out) {
					iState = ItemParsingState.Transform1;
					itemNameEnd = c - 1;
				} else if (it.Current == '>' && iState == ItemParsingState.Transform1 && aState == ApostropheState.Out) {
					iState = ItemParsingState.Transform2;
				} else if (iState == ItemParsingState.Transform2 && aState == ApostropheState.Out && c == sourceWithoutParens.Length - 1) {
					transformEnd = c;
				} else if (iState == ItemParsingState.Transform2 && aState == ApostropheState.Out && it.Current == ',') {
					transformEnd = c - 1;
					separatorStart = c + 1;
					break;
				} else if (iState == ItemParsingState.Name && aState == ApostropheState.Out && it.Current == ',') {
					separatorStart = c + 1;
					itemNameEnd = c - 1;
					break;
				}
			}
			itemName = sourceWithoutParens.Substring (0, itemNameEnd + 1);
			if (transformEnd != -1) {
				if (separatorStart != -1) {
					separator = sourceWithoutParens.Substring (separatorStart + 1, sourceWithoutParens.Length
						- separatorStart - 2);
					transform = new Expression (parent.Project, sourceWithoutParens.Substring (itemNameEnd + 4,
					transformEnd - itemNameEnd - 4));
				} else {
					transform = new Expression (parent.Project, sourceWithoutParens.Substring (itemNameEnd + 4,
					sourceWithoutParens.Length - itemNameEnd - 5));
				}
			} else {
				if (separatorStart != -1) {
					separator = sourceWithoutParens.Substring (separatorStart + 1, sourceWithoutParens.Length
						- separatorStart - 2);
				}
			}
		}
		
		public ITaskItem[] ToITaskItemArray ()
		{
			if (itemName != String.Empty) {
				Project p = parent.Project;
				BuildItemGroup big;
				if (p.EvaluatedItemsByName.Contains (itemName)) {
					big = (BuildItemGroup)p.EvaluatedItemsByName [itemName];
					return big.ToITaskItemArray (transform);
				} else
					return null;
			} else
				return null;
		}
		
		public new string ToString ()
		{
			if (itemName != String.Empty) {
				Project p = parent.Project;
				BuildItemGroup big;
				if (p.EvaluatedItemsByName.Contains (itemName)) {
					big = (BuildItemGroup)p.EvaluatedItemsByName [itemName];
					return big.ToString (transform, separator);
				} else
					return String.Empty;
			} else
				return String.Empty;
		}
		
		public string ItemName {
			get { return itemName; }
		}
		
		public string Separator {
			get { return separator; }
		}
	}
}

#endif
