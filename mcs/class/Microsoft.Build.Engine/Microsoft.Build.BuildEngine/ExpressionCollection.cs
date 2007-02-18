//
// ExpressionCollection.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2006 Marek Sieradzki
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
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {

	internal class ExpressionCollection {
	
		IList objects;
	
		public ExpressionCollection ()
		{
			objects = new ArrayList ();
		}
		
		public void Add (ItemReference itemReference)
		{
			objects.Add (itemReference);
		}
		
		public void Add (MetadataReference metadataReference)
		{
			objects.Add (metadataReference);
		}
		
		public void Add (PropertyReference propertyReference)
		{
			objects.Add (propertyReference);
		}
		
		public void Add (string s)
		{
			objects.Add (s);
		}
		
		public object ConvertTo (Project project, Type type)
		{
			if (type.IsArray) {
				if (type == typeof (ITaskItem[]))
					return ConvertToITaskItemArray (project);
				else
					return ConvertToArray (project, type);
			} else {
				if (type == typeof (ITaskItem))
					return ConvertToITaskItem (project);
				else
					return ConvertToNonArray (project, type);
			}
		}
		
		public IEnumerator GetEnumerator ()
		{
			foreach (object o in objects)
				yield return o;
		}
		
		object ConvertToNonArray (Project project, Type type)
		{
			return ConvertToObject (ConvertToString (project), type);
		}

		object ConvertToArray (Project project, Type type)
		{
			string[] rawTable = ConvertToString (project).Split (';');
			int i = 0;
			
			if (type == typeof (bool[])) {
				bool[] array = new bool [rawTable.Length];
				foreach (string raw in rawTable)
					array [i++] = (bool) ConvertToObject (raw, type.GetElementType ());
				return array;
			} else if (type == typeof (string[])) {
				string[] array = new string [rawTable.Length];
				foreach (string raw in rawTable)
					array [i++] = (string) ConvertToObject (raw, type.GetElementType ());
				return array;
			} else if (type == typeof (int[])) {
				int[] array = new int [rawTable.Length];
				foreach (string raw in rawTable)
					array [i++] = (int) ConvertToObject (raw, type.GetElementType ());
				return array;
			} else if (type == typeof (uint[])) {
				uint[] array = new uint [rawTable.Length];
				foreach (string raw in rawTable)
					array [i++] = (uint) ConvertToObject (raw, type.GetElementType ());
				return array;
			} else if (type == typeof (DateTime[])) {
				DateTime[] array = new DateTime [rawTable.Length];
				foreach (string raw in rawTable)
					array [i++] = (DateTime) ConvertToObject (raw, type.GetElementType ());
				return array;
			} else
				throw new Exception ("Invalid type");
		}
		
		object ConvertToObject (string raw, Type type)
		{
			if (type == typeof (bool)) {
				return Boolean.Parse (raw);
			} else if (type == typeof (string)) {
				return raw;
			} else if (type == typeof (int)) {
				return Int32.Parse (raw);
			} else if (type == typeof (uint)) {
				return UInt32.Parse (raw);
			} else if (type == typeof (DateTime)) {
				return DateTime.Parse (raw);
			} else {
				throw new Exception (String.Format ("Unknown type: {0}", type.ToString ()));
			}
		}
		string ConvertToString (Project project)
		{
			StringBuilder sb = new StringBuilder ();
			
			foreach (object o in objects) {
				if (o is string) {
					sb.Append ((string) o);
				} else if (o is ItemReference) {
					ItemReference ir = (ItemReference) o;
					sb.Append (ir.ConvertToString (project));
				} else if (o is PropertyReference) {
					PropertyReference pr = (PropertyReference) o;
					sb.Append (pr.ConvertToString (project));
				} else if (o is MetadataReference) {
					// FIXME: we don't handle them yet
				} else {
					throw new Exception ("Invalid type in objects collection.");
				}
			}
			return sb.ToString ();
		}

		ITaskItem ConvertToITaskItem (Project project)
		{
			ITaskItem item;
			
			if (objects == null)
				throw new Exception ("Cannot cast empty expression to ITaskItem.");
			
			if (objects [0] is ItemReference) {
				ItemReference ir = (ItemReference) objects [0];
				ITaskItem[] array = ir.ConvertToITaskItemArray (project);
				if (array.Length == 1) {
					return array [0];
				} else {
					throw new Exception ("TaskItem array too long");
				}
			} else {
				item = new TaskItem (ConvertToString (project));
				return item;
			}
		}
		
		ITaskItem[] ConvertToITaskItemArray (Project project)
		{
			List <ITaskItem> finalItems = new List <ITaskItem> ();
			ArrayList tempItems = new ArrayList ();
			ITaskItem[] array;
			
			foreach (object o in objects) {
				if (o is ItemReference) {
					tempItems.Add (o);
				} else if (o is PropertyReference) {
					PropertyReference pr = (PropertyReference) o;
					tempItems.Add (pr.ConvertToString (project));
				} else if (o is MetadataReference) {
					// FIXME: not handled yet
				} else if (o is string) {
					tempItems.Add (o);
				} else {
					throw new Exception ("Invalid type in objects collection.");
				}
			}
			foreach (object o in tempItems) {
				if (o is ItemReference) {
					ItemReference ir = (ItemReference) o;
					array = ir.ConvertToITaskItemArray (project);
					if (array != null)
						finalItems.AddRange (array);
				} else if (o is string) {
					string s = (string) o;
					array = ConvertToITaskItemArrayFromString (project, s);
					finalItems.AddRange (array);
				} else {
					throw new Exception ("Invalid type in tempItems collection.");
				}
			}
			
			return finalItems.ToArray ();
		}
		
		ITaskItem [] ConvertToITaskItemArrayFromString (Project project, string source)
		{
			List <ITaskItem> items = new List <ITaskItem> ();
			string [] splitSource = source.Split (';');
			foreach (string s in splitSource) {
				if (s != String.Empty) {
					items.Add (new TaskItem (s));
				}
			}
			return items.ToArray ();
		}
	}
}

#endif
