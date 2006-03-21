//
// ChangeType.cs: Changes types for output properties or items.
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
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class ChangeType {
	
		private static string TemporaryTransform (object o, Type type)
		{
			string output = String.Empty;
			if (type == typeof (bool)) {
				bool t = (bool) o;
				output =  t.ToString (); 
			} else if (type == typeof (string)) {
				string t = (string) o;
				output =  t.ToString ();
			} else if (type == typeof (DateTime)) {
				DateTime t = (DateTime) o;
				output =  t.ToString ();
			} else if (type == typeof (int)) {
				int t = (int) o;
				output =  t.ToString ();
			} else if (type == typeof (uint)) {
				uint t = (uint) o;
				output =  t.ToString ();
			} else {
			}
			return output;
		}
		
		public static string TransformToString (object o, Type type)
		{
			return TemporaryTransform (o, type);
		}
		
		public static string TransformToString (object[] o, Type type)
		{
			ArrayList al = new ArrayList ();
			foreach (object obj in o ) {
				if (type == typeof (bool[])) {
					al.Add (TemporaryTransform (obj, typeof (bool)));
				} else if (type == typeof (string[])) {
					al.Add (TemporaryTransform (obj, typeof (string)));
				} else if (type == typeof (DateTime[])) {
					al.Add (TemporaryTransform (obj, typeof (DateTime)));
				} else if (type == typeof (int[])) {
					al.Add (TemporaryTransform (obj, typeof (int)));
				} else if (type == typeof (uint[])) {
					al.Add (TemporaryTransform (obj, typeof (uint)));
				} else
					throw new Exception (String.Format ("Invalid type: {0}", type.ToString ()));
			}
			string[] output = new string [al.Count];
			int i  = 0;
			foreach (string s in al)
				output [i++] = s;
			return String.Join (";", output);
		}
		
		public static BuildProperty TransformToBuildProperty (string name, string items)
		{
			return new BuildProperty (name, items);
		}
		
		public static BuildProperty TransformToBuildProperty (string name, ITaskItem[] items)
		{
			BuildProperty buildProperty;
			buildProperty = new BuildProperty (name, TransformToString (items));
			return buildProperty;
		}
		
		public static BuildProperty TransformToBuildProperty (string name, ITaskItem item)
		{
			BuildProperty buildProperty;
			buildProperty = new BuildProperty (name, TransformToString (item));
			return buildProperty;
		}
		
		public static BuildItemGroup TransformToBuildItemGroup (string name, string items)
		{
			string[] splittedItems = items.Split (';');
			BuildItemGroup big = new BuildItemGroup ();
			foreach (string item in splittedItems)
				big.AddItem (name, new TaskItem (item));
			return big;
		}
		
		public static BuildItemGroup TransformToBuildItemGroup (string name, ITaskItem[] items)
		{
			BuildItemGroup big = new BuildItemGroup ();
			foreach (ITaskItem item in items) {
				big.AddItem (name, item);
			}
			return big;
		}
		
		public static BuildItemGroup TransformToBuildItemGroup (string name, ITaskItem item)
		{
			BuildItemGroup big = new BuildItemGroup ();
			big.AddItem (name, item);
			return big;
		}
		
		private static string TransformToString (ITaskItem[] items)
		{
			string[] text = new string [items.Length];
			int i = 0;
			foreach (ITaskItem item in items)
				text [i++] = item.ItemSpec;
			return String.Join (";", text);
		}
		
		private static string TransformToString (ITaskItem item)
		{
			return item.ItemSpec;
		}
	}
}

#endif
