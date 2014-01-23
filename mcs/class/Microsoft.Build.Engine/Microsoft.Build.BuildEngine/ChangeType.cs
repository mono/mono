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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class ChangeType {
		//removed Type type
		// FIXME throw exception here
		static string ToString (object o)
		{
			string output = null;
			Type type = o.GetType ();

			// FIXME: there are more types
			if (type == typeof (string))
				output = (string) o;
			else if (type == typeof (bool) ||
				 type == typeof (int) ||
				 type == typeof (uint) ||
				 type == typeof (float) ||
				 type == typeof (double) ||
				 type == typeof (DateTime) ||
				 type.IsEnum)
				output = o.ToString ();
			else
				throw new Exception (String.Format ("Unsupported type : {0}", type));
			return output;
		}

		static string ToString (object [] o, Type type)
		{
		/*
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
			*/

			List <string> list = new List <string> ();
			foreach (object obj in o)
				list.Add (ToString (obj));
			return String.Join (";", list.ToArray ());
		}

		public static BuildProperty ToBuildProperty (object o, Type t, string name)
		{
			if (t == typeof (ITaskItem)) {
				return new BuildProperty (name, ((ITaskItem) o).ItemSpec);
			} else if (t ==  typeof (ITaskItem [])) {
			// FIXME move Tostring here
				return new BuildProperty (name, ToString ((ITaskItem []) o));
			} else if (t.IsArray) {
				return new BuildProperty (name, ToString ((object []) o, t));
			} else {
				return new BuildProperty (name, ToString (o));
			}
		}

		public static BuildItemGroup ToBuildItemGroup (object o, Type t, string name)
		{
			BuildItemGroup big = new BuildItemGroup ();

			if (t == typeof (ITaskItem)) {
				big.AddItem (name, (ITaskItem) o);
			} else if (t ==  typeof (ITaskItem [])) {
				foreach (ITaskItem i in (ITaskItem []) o)
					big.AddItem (name, i);
			} else if (t.IsArray) {
				return ToBuildItemGroup (name, ToString ((object []) o, t), true);
			} else {
				return ToBuildItemGroup (name, ToString (o), false);
			}

			return big;
		}
		
		static BuildItemGroup ToBuildItemGroup (string name, string items, bool split)
		{
			BuildItemGroup big = new BuildItemGroup ();
			if (split) {
				string [] splitItems = items.Split (';');
				foreach (string item in splitItems)
					big.AddItem (name, new TaskItem (item));
			} else {
				big.AddItem (name, new TaskItem (items));
			}

			return big;
		}
		
		static string ToString (ITaskItem [] items)
		{
			string [] text = new string [items.Length];
			int i = 0;
			foreach (ITaskItem item in items)
				text [i++] = item.ItemSpec;
			return String.Join (";", text);
		}
	}
}
