// 
// genxs.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
//

using System;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

public class Driver
{
	static void Main (string[] args)
	{
		if (args.Length == 0 || args[0] == "--help")
		{
			Console.WriteLine ("Mono Xml Serializer Generator Tool");
			Console.WriteLine ("Usage: genxs ConfigFileName [DestinationPath]");
			Console.WriteLine ();
			return;
		}

		try
		{
			Type t = Type.GetType ("System.Xml.Serialization.SerializationCodeGenerator, System.Xml");
			if (t == null) throw new Exception ("This runtime does not support generation of serializers");
		
			MethodInfo met = t.GetMethod ("Generate", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			met.Invoke (null, new object[] {args[0], (args.Length > 1) ? args[1] : null} );
		}
		catch (Exception ex)
		{
			Console.WriteLine ("An error occurred while generating serializers: " + ex);
		}
	}
}
