//
// System.Drawing.PaperSource.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
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
		//[MonoTODO]
		public PaperSourceKind Kind{
			get {
			throw new NotImplementedException ();			}
		}
		//[MonoTODO]
		public string SourceName{
			get {
			throw new NotImplementedException ();			}
		}
		//[MonoTODO]
		public override string ToString(){
			return SourceName;
		}
	}
}
