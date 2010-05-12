//
// AssociationAttribute.cs
//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell Inc. (http://novell.com)
//

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
#if NET_4_0
using System;
using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class AssociationAttribute : Attribute
	{
		static readonly char[] keySplitChars = { ',' };
		
		IEnumerable <string> otherKeyMembers;
		IEnumerable <string> thisKeyMembers;
		
		public bool IsForeignKey { get; set; }
		public string Name { get; private set; }
		public string OtherKey { get; private set; }
		
		public IEnumerable<string> OtherKeyMembers {
			get {
				if (otherKeyMembers == null)
					otherKeyMembers = GetKeyMembers (OtherKey);

				return otherKeyMembers;
			}
		}
		
		public string ThisKey { get; private set; }
		
		public IEnumerable<string> ThisKeyMembers {
			get {
				if (thisKeyMembers == null)
					thisKeyMembers = GetKeyMembers (ThisKey);

				return thisKeyMembers;
			}
		}
		
		public AssociationAttribute (string name, string thisKey, string otherKey)
		{
			this.Name = name;
			this.ThisKey = thisKey;
			this.OtherKey = otherKey;
		}

		IEnumerable <string> GetKeyMembers (string key)
		{
			// .NET emulation
			if (key == null)
				throw new NullReferenceException (".NET emulation");

			string nows = key.Replace (" ", String.Empty);
			if (nows.Length == 0)
				return new string[] { String.Empty };
					
			return nows.Split (keySplitChars);
		}
	}
}
#endif
