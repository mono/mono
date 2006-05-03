//
// OldExpression.cs: Stores references to items or properties.
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
	internal class OldExpression {
	
		Project	project;
		ItemReference parentItemReference;
		ExpressionCollection expressionCollection;
	
		public OldExpression (Project project)
		{
			this.project = project;
			this.expressionCollection = new ExpressionCollection ();
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
							expressionCollection.Add (temp.ToString ());
							
							temp = new StringBuilder ();
						}
						eState = EvaluationState.InItem;
						start = current;
						break;
					case '$':
						if (temp.Length > 0) {
							expressionCollection.Add (temp.ToString ());
							temp = new StringBuilder ();
						}
						eState = EvaluationState.InProperty;
						start = current;
						break;
					case '%':
						if (temp.Length > 0) {
							expressionCollection.Add (temp.ToString ());
							temp = new StringBuilder ();
						}
						eState = EvaluationState.InMetadata;
						start = current;
						break;
					default:
						temp.Append (it.Current);
						if (current == source.Length - 1)
							expressionCollection.Add (temp.ToString ());
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
							ItemReference ir = new ItemReference (this);
							ir.ParseSource (source.Substring (start, current - start + 1));
							expressionCollection.Add (ir);
							
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
							PropertyReference pr = new PropertyReference (this);
							pr.ParseSource (source.Substring (start, current - start + 1));
							expressionCollection.Add (pr);
							
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
							MetadataReference mr = new MetadataReference (this);
							mr.ParseSource (source.Substring (start, current - start + 1));
							expressionCollection.Add (mr);
							
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
		
		public object ConvertTo (Type type)
		{
			return expressionCollection.ConvertTo (type);
		}

		public Project Project {
			get { return project; }
		}

		public ExpressionCollection Collection {
			get { return expressionCollection; }
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
