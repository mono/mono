//
// FormalParameterList.cs: A list of identifiers.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System.Collections;
	using System.Text;

	public class FormalParameterList
	{
		internal ArrayList ids;

		public FormalParameterList ()
		{
			ids = new ArrayList ();
		}

		internal void Add (string id)
		{	
			ids.Add (id);	
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
		
			foreach (string s in ids)
				sb.Append (s + " ");
		
			return sb.ToString ();
		}
	}
}