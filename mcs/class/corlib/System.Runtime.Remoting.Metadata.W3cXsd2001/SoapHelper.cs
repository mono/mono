//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNmtoken
//
// Authors:
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Novell, Inc.
//

using System;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	internal class SoapHelper
	{
		public static Exception GetException (ISoapXsd type, string msg)
		{
			return new RemotingException ("Soap Parse error, xsd:type xsd:" + type.GetXsdType() + " " + msg);
		}
		
		public static string Normalize (string s)
		{
			return s;
		}
	}
}

