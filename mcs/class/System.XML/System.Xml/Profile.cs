// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// Profile.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

using System;
using System.Xml;

using System.IO;
using System.Text;

public class Profile
{
	public static void Main(string[] args)
	{
		XmlReader xmlReader = null;

		if (args.Length < 1)
		{
			xmlReader = new XmlTextReader(Console.In);
		}
		else
		{
			xmlReader = new XmlTextReader(args[0]);
		}

		int nodes = 0;

		DateTime start = DateTime.Now;

		while (xmlReader.Read())
		{
			++nodes;
		}

		DateTime end = DateTime.Now;

		Console.WriteLine("time = {0}", end - start);

		Console.WriteLine("nodes = {0}", nodes);
	}
}
