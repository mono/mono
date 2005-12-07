//
// System.Drawing.PaperSource.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//
// (C) 2002 Ximian, Inc
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Drawing.Printing
{
	/// <summary>
	/// Summary description for PaperSource.
	/// </summary>
#if NET_2_0
	[Serializable]
#endif	
	public class PaperSource
	{
		PaperSourceKind _Kind;
		string _SourceName;
		
#if NET_2_0
		public PaperSource ()
		{
			
		}
#endif
		// NOTE:how to construct this class?
		// I have added a constructor, but I am not sure of me...
		internal PaperSource(string sourceName, PaperSourceKind kind)
		{
			_SourceName = sourceName;
			_Kind = kind;
		}

		public PaperSourceKind Kind{
			get {
				return _Kind; 
			}
		}
		public string SourceName{
			get {
				return _SourceName;
			}
#if NET_2_0
		set {
				_SourceName = value;
			}
#endif
		}

#if NET_2_0
		[MonoTODO]
		public int RawKind {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}		  
#endif

		public override string ToString(){
			string ret = "[PaperSource {0} Kind={1}]";
			return String.Format(ret, this.SourceName, this.Kind);
		}
	}
}
