//
// System.Drawing.PaperSource.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing.Printing
{
	/// <summary>
	/// Summary description for PaperSource.
	/// </summary>
	public class PaperSource
	{
		PaperSourceKind _Kind;
		string _SourceName;
		
		// NOTE:how to construct this class?
		// I have added a constructor, but I am not sure of me...
		internal PaperSource(string sourceName, PaperSourceKind kind)
		{
			_SourceName = sourceName;
			_Kind = kind;
		}
		
		public PaperSourceKind Kind{
			get {
			return _Kind; }
		}
		public string SourceName{
			get {
			return _SourceName; }
		}
		public override string ToString(){
			string ret = "[PaperSource {0} Kind={1}]";
			return String.Format(ret, this.SourceName, this.Kind);
		}
	}
}
