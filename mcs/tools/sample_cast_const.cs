using System;

class X {
	static void w (string s)
	{
		Console.WriteLine (s);
	}
	
	static void Main ()
	{
		object [,] names = 
			{ { "Byte", "byte" },
			  { "SByte", "sbyte" },
			  { "Short", "short" },
			  { "UShort", "ushort" },
			  { "Int", "int32" },
			  { "UInt", "uint32" },
			  { "Long", "int64" },
			  { "ULong", "uint64" },
			  { "Float", "float" },
			  { "Double", "double" },
			  { null, null }
			};

		for (int i = 0; names [i,0] != null; i++){
			string big = names [i, 0] + "Constant";
			string small = "TypeManager." + names [i, 1] + "_type";
			string nat = ((string) names [i,0]).ToLower ();
			
			w ("\t\tif (expr is " + big + "Constant){");
			w ("\t\t\t" + big + " v = ((" + big + ") expr).Value;");
			w ("");

			for (int j = 0; names [j,0] != null; j++){
				if (i == j)
					continue;
				
				w ("\t\t\tif (target_type == " + small + ")");
				w ("\t\t\t\treturn new " + big + "((" + nat + ") v);");
			}
			w ("\t\t}");
		}
	}
}
	
