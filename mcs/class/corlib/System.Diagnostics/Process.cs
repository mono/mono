//
// System.Diagnostics.Process 
//
// 	Authors:
// 		Jaime Anguiano Olarra (jaime@gnome.org)
//
// (C) 2002, Jaime Anguiano Olarra
//
// FIXME: This is just a skeleton class for practical purposes
//

using System;
using System.Reflection;

namespace System.Diagnostics 
{
	[MonoTODO]
	public class Process 
	{
		int id;
	
		// This way of constructing the id for the process
		// is absolutly mine so it probably won't be 
		// something you should trust on.
		public Process ()
		{
			// We get the current executing assembly
			// The specs say that the process id doesn't
			// exists if the associated process is not 
			// running.
			Assembly cea = Assembly.GetExecutingAssembly ();
			id = Convert.ToInt32 (cea.FullName);
		}
		
		public static Process GetCurrentProcess ()
		{
			return new Process ();
		}

		public int Id
		{
			get { return id; }
		}
	}
}
