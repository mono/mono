//
// ResInfo.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class ResInfo
	{
		public string filename;
		public string fullpath;
		public string name;
		public bool isPublic;
		public bool isLinked;

		public ResInfo (string filename, string  name, bool isPublic, bool isLinked)
		{
			throw new NotImplementedException ();
		}


		public ResInfo (string resInfo, bool isLinked)
		{
			throw new NotImplementedException ();
		}
	}
}