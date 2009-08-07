//
// CSharpArgumentInfo.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CSharp.RuntimeBinder
{
	public class CSharpArgumentInfo
	{
		CSharpArgumentInfoFlags flags;
		string name;
		
		public CSharpArgumentInfo (CSharpArgumentInfoFlags flags, string name)
		{
			this.flags = flags;
			this.name = name;
		}
		
		public override bool Equals (object obj)
		{
			var other = obj as CSharpArgumentInfo;
			return other != null && other.name == name && other.flags == flags;
		}

		public CSharpArgumentInfoFlags Flags {
			get { return flags; }
		}
		
		public override int GetHashCode ()
		{
			return EqualityComparer<string>.Default.GetHashCode (name) ^ flags.GetHashCode ();
		}
		
		internal bool IsNamed {
			get { return (flags & CSharpArgumentInfoFlags.NamedArgument) != 0; }
		}

		public string Name {
			get { return name; }
		}
		
		internal static CallInfo CreateCallInfo (IEnumerable<CSharpArgumentInfo> argumentInfo, int skipCount)
		{
			var named = from arg in argumentInfo.Skip (skipCount) where arg.IsNamed select arg.Name;
			return new CallInfo (Math.Max (0, argumentInfo.Count () - skipCount), named);
		}
	}
}
