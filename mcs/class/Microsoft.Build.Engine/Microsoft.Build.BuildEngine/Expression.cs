//
// Expression.cs: Stores references to items or properties.
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
using System.IO;
using System.Collections;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class Expression {
	
		IList	objects;
		Project	project;
		ItemReference parentItemReference;
	
		public Expression (Project project)
		{
			this.objects = new ArrayList ();
			this.project = project;
		}
		
		public Expression (Project project, string source)
			: this (project)
		{
			ParseSource (source);
		}
		
		public void ParseSource (string source)
		{
			// FIXME: change StringBuilder to substrings 
			if (source == null)
				throw new ArgumentNullException ("source");				

			// FIXME: hack
			source = source.Replace ('/', Path.DirectorySeparatorChar);
			source = source.Replace ('\\', Path.DirectorySeparatorChar);
			StringBuilder temp = new StringBuilder ();
			CharEnumerator it = source.GetEnumerator ();
			EvaluationState eState = EvaluationState.Out;
			ParenState pState = ParenState.Out;
			ApostropheState aState = ApostropheState.Out;
			int start = 0;
			int current = -1;
			
			while (it.MoveNext ()) {
				current++;
				switch (eState) {
				case EvaluationState.Out:
					switch (it.Current) {
					case '@':
						if (temp.Length > 0) {
							objects.Add (temp.ToString ());
							temp = new StringBuilder ();
						}
						eState = EvaluationState.InItem;
						start = current;
						break;
					case '$':
						if (temp.Length > 0) {
							objects.Add (temp.ToString ());
							temp = new StringBuilder ();
						}
						eState = EvaluationState.InProperty;
						start = current;
						break;
					case '%':
						if (temp.Length > 0) {
							objects.Add (temp.ToString ());
							temp = new StringBuilder ();
						}
						eState = EvaluationState.InMetadata;
						start = current;
						break;
					default:
						temp.Append (it.Current);
						if (current == source.Length - 1)
							objects.Add (temp.ToString ());
						break;
					}
					break;
				case EvaluationState.InItem:
					switch (it.Current) {
					case '(':
						if (pState == ParenState.Out && aState == ApostropheState.Out)
							pState = ParenState.Left;
						else if (aState == ApostropheState.Out)
							throw new Exception ("'(' not expected.");
						break;
					case ')':
						if (pState == ParenState.Left && aState == ApostropheState.Out) {
							objects.Add (new ItemReference (this, source.Substring (start, current - start + 1)));
							eState = EvaluationState.Out;
							pState = ParenState.Out;
						}
						break;
					case '\'':
						if (aState == ApostropheState.In)
							aState = ApostropheState.Out;
						else
							aState = ApostropheState.In;
						break;
					default:
						break;
					}
					break;
				case EvaluationState.InProperty:
					switch (it.Current) {
					case '(':
						if (pState == ParenState.Out)
							pState = ParenState.Left;
						else
							throw new Exception ("'(' expected.");
						break;
					case ')':
						if (pState == ParenState.Left) {
							objects.Add (new PropertyReference (this, source.Substring (start, current - start + 1)));
							eState = EvaluationState.Out;
							pState = ParenState.Out;
						}
						break;
					default:
						break;
					}
					break;
				case EvaluationState.InMetadata:
					switch (it.Current) {
					case '(':
						if (pState == ParenState.Out)
							pState = ParenState.Left;
						break;
					case ')':
						if (pState == ParenState.Left) {
							objects.Add (new MetadataReference (this, source.Substring (start, current - start + 1)));
							eState = EvaluationState.Out;
							pState = ParenState.Out;
						}
						break;
					default:
						break;
					}
					break;
				default:
					throw new Exception ("Invalid evaluation state.");
				}
			}
		}

		public IEnumerator GetEnumerator ()
		{
			foreach (object o in objects)
				yield return o;
		}
		
		public object ToNonArray (Type type)
		{
			if (type.IsArray == true)
				throw new ArgumentException ("Type specified can not be array type.");
			
			return ToObject (ToString (), type);
		}
		
		public object ToArray (Type type)
		{
			if (type.IsArray == false)
				throw new ArgumentException ("Type specified can not be element type.");
			
			string[] rawTable = ToString ().Split (';');
			int i = 0;
			
			if (type == typeof (bool[])) {
				bool[] array = new bool [rawTable.Length];
				foreach (string raw in rawTable)
					array [i++] = (bool) ToObject (raw, typeof (bool));
				return array;
			} else if (type == typeof (string[])) {
				string[] array = new string [rawTable.Length];
				foreach (string raw in rawTable)
					array [i++] = (string) ToObject (raw, typeof (string));
				return array;
			} else if (type == typeof (int[])) {
				int[] array = new int [rawTable.Length];
				foreach (string raw in rawTable)
					array [i++] = (int) ToObject (raw, typeof (int));
				return array;
			} else if (type == typeof (uint[])) {
				uint[] array = new uint [rawTable.Length];
				foreach (string raw in rawTable)
					array [i++] = (uint) ToObject (raw, typeof (uint));
				return array;
			} else if (type == typeof (DateTime[])) {
				DateTime[] array = new DateTime [rawTable.Length];
				foreach (string raw in rawTable)
					array [i++] = (DateTime) ToObject (raw, typeof (DateTime));
				return array;
			} else throw new Exception ("Invalid type.");
		}
		
		private object ToObject (string raw, Type type)
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
		
		private new string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			
			foreach (object o in this) {
				if (o is string) {
					sb.Append ((string) o);
				} else if (o is ItemReference) {
					sb.Append (((ItemReference)o).ToString ());
				} else if (o is PropertyReference) {
					sb.Append (((PropertyReference)o).ToString ());
				} else if (o is MetadataReference) {
					// FIXME: we don't handle them yet
				} else {
					throw new Exception ("Invalid type in objects collection.");
				}
			}
			return sb.ToString ();
		}

		public ITaskItem ToITaskItem ()
		{
			ITaskItem item;
			
			if (objects == null)
				throw new Exception ("Cannot cast empty expression to ITaskItem.");
			
			if (objects [0] is ItemReference) {
				ItemReference ir = (ItemReference) objects [0];
				ITaskItem[] array = ir.ToITaskItemArray ();
				if (array.Length == 1) {
					return array [0];
				} else {
					throw new Exception ("TaskItem array too long");
				}
			} else {
				item = new TaskItem (ToString ());
				return item;
			}
		}
		
		public ITaskItem[] ToITaskItemArray ()
		{
			ArrayList finalItems = new ArrayList ();
			ArrayList tempItems = new ArrayList ();
			ITaskItem[] array;
			ITaskItem[] finalArray;
			
			foreach (object o in objects) {
				if (o is ItemReference) {
					tempItems.Add (o);
				} else if (o is PropertyReference) {
					PropertyReference pr = (PropertyReference) o;
					tempItems.Add (pr.ToString ());
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
					array = ir.ToITaskItemArray ();
					if (array != null)
						foreach (ITaskItem item in array)
							finalItems.Add (item);
				} else if (o is string) {
					string s = (string) o;
					array = ITaskItemArrayFromString (s);
					foreach (ITaskItem item in array)
						finalItems.Add (item);
				} else {
					throw new Exception ("Invalid type in tempItems collection.");
				}
			}
			
			finalArray = new ITaskItem [finalItems.Count];
			int i = 0;
			foreach (ITaskItem item in finalItems)
				finalArray [i++] = item;
			return finalArray;
		}
		
		// FIXME: quite stupid name
		private ITaskItem[] ITaskItemArrayFromString (string source)
		{
			ArrayList tempItems = new ArrayList ();
			ITaskItem[] finalArray;
			string[] splittedSource = source.Split (';');
			foreach (string s in splittedSource) {
				if (s != String.Empty) {
					tempItems.Add (new TaskItem (s));
				}
			}
			finalArray = new ITaskItem [tempItems.Count];
			int i = 0;
			foreach (ITaskItem item in tempItems)
				finalArray [i++] = item;
			return finalArray;
		}
		
		public Project Project {
			get { return project; }
		}
		
		public ItemReference ParentItemReference {
			get { return parentItemReference; }
		}
	}

	internal enum EvaluationState {
		Out,
		InItem,
		InMetadata,
		InProperty
	}
	
	internal enum ParenState {
		Out,
		Left
	}
	
	internal enum ApostropheState {
		In,
		Out
	}
	
	internal enum ItemParsingState {
		Name,
		Transform1,
		Transform2,
		Separator
	}
}

#endif
