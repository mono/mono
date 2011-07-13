// 
// PathExtensions.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths {
	static class PathExtensions {
		public static HashSet<Field> FieldsIn (this LispList<PathElement> path)
		{
			var result = new HashSet<Field> ();
			if (path != null) {
				foreach (PathElement element in path.AsEnumerable ()) {
					Field f;
					if (element.TryField (out f))
						result.Add (f);
				}
			}
			return result;
		}

		public static string ToCodeString (this PathElement[] path)
		{
			return PathToString (path);
		}

		public static string ToCodeString (this LispList<PathElement> path)
		{
			return PathToString (path.AsEnumerable ());
		}

		private static string PathToString (IEnumerable<PathElement> path)
		{
			bool first = true;
			bool isReference = false;
			bool isUnmanagedPointer = false;
			var sb = new StringBuilder ();

			List<PathElement> pathL = path.ToList ();

			for (int i = 0; i < pathL.Count; i++) {
				PathElement element = pathL [i];
				if (element.IsMethodCall && !element.IsGetter && element.IsStatic) {
					string oldString = sb.ToString ();
					sb = new StringBuilder ();
					sb.AppendFormat ("{0}({1})", element, oldString);
				} else {
					if (!string.IsNullOrEmpty (element.CastTo)) {
						string oldString = sb.ToString ();
						sb = new StringBuilder ();
						sb.AppendFormat ("(({0}{1}){2})", element.CastTo, isUnmanagedPointer ? "*" : "", oldString);
					}

					sb.Append (isUnmanagedPointer ? "->" : ".");
					sb.Append (element.ToString ());
					if (element.IsMethodCall && !element.IsGetter)
						sb.Append ("()");
				}
				if (first)
					first = false;

				int num = (element.IsAddressOf ? 1 : 0) + (element.IsUnmanagedPointer ? 1 : 0) + (element.IsManagedPointer ? 1 : 0);
				isUnmanagedPointer = element.IsUnmanagedPointer;

				for (int j = 0; j < num; j++) {
					if (j + 1 < pathL.Count) {
						if (pathL [j + 1].IsDeref)
							++j;
					} else
						isReference = true;
				}
			}

			if (isReference)
				return isUnmanagedPointer ? sb.ToString () : "&" + sb;
			if (isUnmanagedPointer)
				return "*" + sb;

			return sb.ToString ();
		}
	}
}
