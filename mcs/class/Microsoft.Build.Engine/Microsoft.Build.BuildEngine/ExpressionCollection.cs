//
// ExpressionCollection.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
// 
// (C) 2006 Marek Sieradzki
// Copyright 2009 Novell, Inc (http://www.novell.com)
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
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {

	internal class ExpressionCollection {
	
		IList objects;
		static Dictionary<string, bool> boolValues;

		static ExpressionCollection ()
		{
			string[] trueValuesArray = new string[] {"true", "on", "yes"};
			string[] falseValuesArray = new string[] {"false", "off", "no"};

			boolValues = new Dictionary<string, bool> (StringComparer.OrdinalIgnoreCase);
			foreach (string s in trueValuesArray)
				boolValues.Add (s, true);
			foreach (string s in falseValuesArray)
				boolValues.Add (s, false);
		}
	
		public ExpressionCollection ()
		{
			objects = new ArrayList ();
		}

		public int Count {
			get { return objects.Count; }
		}
		
		public void Add (IReference reference)
		{
			objects.Add (reference);
		}
		
		public void Add (string s)
		{
			objects.Add (s);
		}
		
		public object ConvertTo (Project project, Type type, ExpressionOptions options)
		{
			if (type.IsArray) {
				if (type == typeof (ITaskItem[]))
					return ConvertToITaskItemArray (project, options);
				else
					return ConvertToArray (project, type, options);
			} else {
				if (type == typeof (ITaskItem))
					return ConvertToITaskItem (project, options);
				else
					return ConvertToNonArray (project, type, options);
			}
		}
		
		public IEnumerator GetEnumerator ()
		{
			foreach (object o in objects)
				yield return o;
		}
		
		object ConvertToNonArray (Project project, Type type, ExpressionOptions options)
		{
			return ConvertToObject (ConvertToString (project, options), type, options);
		}

		object ConvertToArray (Project project, Type type, ExpressionOptions options)
		{
			ITaskItem[] items = ConvertToITaskItemArray (project, options);

			Type element_type = type.GetElementType ();
			Array arr = Array.CreateInstance (element_type, items.Length);
			for (int i = 0; i < arr.Length; i ++)
				arr.SetValue (ConvertToObject (items [i].ItemSpec, element_type, options), i);
			return arr;
		}

		object ConvertToObject (string raw, Type type, ExpressionOptions options)
		{
			if (type == typeof (bool)) {
				bool value;
				if (boolValues.TryGetValue (raw, out value))
					return value;
				else
					return false;
			}

			if (type == typeof (string))
				return raw;

			if (type.IsPrimitive)
				return Convert.ChangeType (raw, type);

			if (type == typeof (DateTime))
				return DateTime.Parse (raw);

			throw new Exception (String.Format ("Unsupported type: {0}", type));
		}

		string ConvertToString (Project project, ExpressionOptions options)
		{
			StringBuilder sb = new StringBuilder ();
			
			foreach (object o in objects) {
				string s = o as string;
				if (s != null) {
					sb.Append (s);
					continue;
				}

				IReference br = o as IReference;
				if (br != null)
					sb.Append (br.ConvertToString (project, options));
				else
					throw new Exception ("BUG: Invalid type in objects collection.");
			}
			return sb.ToString ();
		}

		ITaskItem ConvertToITaskItem (Project project, ExpressionOptions options)
		{
			if (objects == null)
				throw new Exception ("Cannot cast empty expression to ITaskItem.");

			ITaskItem[] items = ConvertToITaskItemArray (project, options);
			if (items.Length > 1)
				//FIXME: msbuild gives better errors
				throw new Exception (String.Format ("Exactly one item required, but got: {0}", items.Length));

			if (items.Length == 0) return null;
			return items [0];
		}
		
		// Concat rules (deduced)
		// - ItemRef can concat only with a string ';' or PropertyRef ending in ';'
		// - MetadataRef can concat with anything other than ItemRef
		// - PropertyRef cannot be right after a ItemRef
		//   PropertyRef concats if it doesn't end in ';'
		// - string cannot concat with ItemRef unless it is ';'.
		//   string concats if it ends in ';'
		ITaskItem[] ConvertToITaskItemArray (Project project, ExpressionOptions options)
		{
			List <ITaskItem> finalItems = new List <ITaskItem> ();
			
			object prev = null;
			bool prev_can_concat = false;

			foreach (object o in objects) {
				bool can_concat = prev_can_concat;

				string str = o as string;
				if (str != null) {
					string trimmed_str = str.Trim ();
					if (!IsSemicolon (str) && trimmed_str.Length > 0 && prev != null && prev is ItemReference)
						// non-empty, non-semicolon string after item ref
						ThrowCantConcatError (prev, str);

					if (trimmed_str.Length == 0 && prev is string && IsSemicolon ((string) prev)) {
						// empty string after a ';', ignore it
						continue;
					}

					// empty string _after_ a itemref, not an error
					prev_can_concat = !(str.Length > 0 && str [str.Length - 1] == ';') && trimmed_str.Length > 0;
					AddItemsToArray (finalItems,
							ConvertToITaskItemArrayFromString (str),
							can_concat);
					prev = o;
					continue;
				}

				IReference br = o as IReference;
				if (br == null)
					throw new Exception ("BUG: Invalid type in objects collection.");

				if (o is ItemReference) {
					if (prev != null && !(prev is string && (string)prev == ";"))
						ThrowCantConcatError (prev, br);

					prev_can_concat = true;
				} else if (o is MetadataReference) {
					if (prev != null && prev is ItemReference)
						ThrowCantConcatError (prev, br);

					prev_can_concat = true;
				} else if (o is PropertyReference) {
					if (prev != null && prev is ItemReference)
						ThrowCantConcatError (prev, br);

					string value = ((PropertyReference) o).GetValue (project);
					prev_can_concat = !(value.Length > 0 && value [value.Length - 1] == ';');
				}

				AddItemsToArray (finalItems, br.ConvertToITaskItemArray (project, options), can_concat);

				prev = o;
			}

			// Trim and Remove empty items
			List<ITaskItem> toRemove = new List<ITaskItem> ();
			for (int i = 0; i < finalItems.Count; i ++) {
				string s = finalItems [i].ItemSpec.Trim ();
				if (s.Length == 0)
					toRemove.Add (finalItems [i]);
				else
					finalItems [i].ItemSpec = s;
			}
			foreach (ITaskItem ti in toRemove)
				finalItems.Remove (ti);
			
			return finalItems.ToArray ();
		}

		// concat's first item in @items to last item in @list if @concat is true
		// else just adds all @items to @list
		void AddItemsToArray (List<ITaskItem> list, ITaskItem[] items, bool concat)
		{
			if (items == null || items.Length == 0)
				return;

			int start_index = 1;
			if (concat && list.Count > 0)
				list [list.Count - 1].ItemSpec += items [0].ItemSpec;
			else
				start_index = 0;

			for (int i = start_index; i < items.Length; i ++)
				list.Add (items [i]);
		}
		
		ITaskItem [] ConvertToITaskItemArrayFromString (string source)
		{
			List <ITaskItem> items = new List <ITaskItem> ();
			string [] splitSource = source.Split (new char [] {';'},
					StringSplitOptions.RemoveEmptyEntries);

			foreach (string s in splitSource)
				items.Add (new TaskItem (s));

			return items.ToArray ();
		}

		bool IsSemicolon (string str)
		{
			return str != null && str.Length == 1 && str [0] == ';';
		}

		void ThrowCantConcatError (object first, object second)
		{
			throw new Exception (String.Format (
					"Can't concatenate Item list with other strings where an item list is " +
					"expected ('{0}', '{1}'). Use semi colon to separate items.",
					first.ToString (), second.ToString ()));
		}

	}
}
