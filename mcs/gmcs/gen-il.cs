// gen-il.cs: Generates MSIL code from the CIR.Tree
//
// Author: Miguel de Icaza (miguel@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.IO;
using System.Collections;
using CIR;

namespace MSIL {

	public class Generator : CIR.ITreeDump {
		StreamWriter o;
		int indent = 0;
		
		void output (string s)
		{
			Console.Write (s);
			o.Write (s);
		}

		void space ()
		{
			output (new String (' ', indent * 2));
		}

		void ioutput (string s)
		{
			space ();
			output (s);
		}

		void ioutputl (string s)
		{
			ioutput (s + "\n");
		}

		string ClassAttributes (Class c)
		{
			// FIXME
			return "";
		}

		string ILName (string name)
		{
			return name;
		}

		string ClassExtends (Class c)
		{
			return "";
		}
		
		void GenerateFromClass (Class c)
		{
			ioutputl (".class " + ClassAttributes (c) + " " + ILName (c.Name));
			ioutputl (ClassExtends (c));
			ioutputl ("{");
			indent++;

			

			indent--;
			ioutputl ("}");
		}
		
		void GenerateFromTypes (TypeContainer types)
		{
			if (types.Types == null)
				return;
			
			foreach (DictionaryEntry de in types.Types){
				TypeContainer type = (TypeContainer) de.Value;
				
				if (type is Class)
					GenerateFromClass ((Class) type);
			}
			
		}
		
		public int GenerateFromTree (Tree tree, StreamWriter os)
		{
			this.o = os;

			ioutputl (".assembly test.exe { }");
			GenerateFromTypes (tree.Types);
			return 0;
		}

		public void ParseOptions (string options)
		{
		}
		
	}
}
