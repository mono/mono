//
// location.cs: Keeps track of the location of source code entity
//
// Author:
//   Miguel de Icaza
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.Collections;

namespace Mono.CSharp {
	/// <summary>
	///   Keeps track of the location in the program
	/// </summary>
	///
	/// <remarks>
	///   This uses a compact representation and a couple of auxiliary
	///   structures to keep track of tokens to (file,line) mappings.
	///
	///   We could probably also keep track of columns by storing those
	///   in 8 bits (and say, map anything after char 255 to be `255+').
	/// </remarks>
	public struct Location {
		public int token; 

		static Hashtable map;
		static ArrayList list;
		static int global_count;
		static int module_base;

		static Location ()
		{
			map = new Hashtable ();
			list = new ArrayList ();
			global_count = 0;
			module_base = 0;
		}
	
		static public void Push (string name)
		{
			map.Add (global_count, name);
			list.Add (global_count);
			module_base = global_count;
		}
		
		public Location (int row)
		{
			if (row < 0)
				token = -1;
			else {
				token = module_base + row;
				if (global_count < token)
					global_count = token;
			}
		}

		public override string ToString ()
		{
			return Name + ": (" + Row + ")";
		}
		
		/// <summary>
		///   Whether the Location is Null
		/// </summary>
		static public bool IsNull (Location l)
		{
			return l.token == -1;
		}

		static public Location Null {
			get {
				return new Location (-1);
			}
		}

		public string Name {
			get {
				int best = 0;
				
				if (token < 0)
					return "Internal";

				foreach (int b in list){
					if (token > b)
						best = b;
				}
				return (string) map [best];
			}
		}

		public int Row {
			get {
				int best = 0;
				
				if (token < 0)
					return 1;
				
				foreach (int b in list){
					if (token > b)
						best = b;
				}
				return token - best;
			}
		}
	}
}
