a//
// Serialize.cs
//
// This program creates a SerializationInfo and requests an object
// to serialize itself.
//
// We serialize because we need to know the *exact* names that are
// used for the values being serialized.
//
// Author: Miguel de Icaza
//	   Duncan Mak
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;

class Driver {
        static object StaticCreateObject ()
        {
                //
                // Change the object type here.
                //
                return null;
        }

	static object LiveCreateObject (Type obj, Type[] types, string[] values)
	{
		if (types.Length != values.Length)
			throw new ArgumentException ();

		object[] a = new object [types.Length];
		
    		for (int i = 0; i < a.Length; i++)
			a [i] = Convert.ChangeType (values [i], types [i]);

		return Activator.CreateInstance (obj, a);
	}
        
        static void Main (string[] args)
        {
		object x = null;
		string strTypes = null;
		string argValues = null;
		
		if (args.Length == 1) {
			Type t = Type.GetType (args[0]);
			Console.WriteLine ("\nPlease enter the arguments to the constructor for type {0}", t.ToString());
			strTypes = Console.ReadLine ();
			Console.WriteLine ("\nPlease enter the values");
			argValues = Console.ReadLine ();
			Type[] types = ToTypeArray (strTypes.Split (','));
			string[] param = argValues.Split (',');
			
			x = LiveCreateObject (t, types, param);
		} else {
			x = StaticCreateObject ();
		}

		string fileName = x.GetType().FullName + ".xml";
		Stream output = new FileStream (fileName, FileMode.Create,
FileAccess.Write, FileShare.None);
                IFormatter formatter = new SoapFormatter ();

                formatter.Serialize ((Stream) output, x);
                output.Close ();
        }

	public static Type[] ToTypeArray (string[] strTypes)
	{
		Type[] t = new Type [strTypes.Length];
		
		for (int i = 0; i < strTypes.Length; i++)
			t [i] = StringToType (strTypes [i]);
		return t;
	}

	public static Type StringToType (string s)
	{
		switch (s)
		{
		case "bool":
			return Type.GetType ("System.Boolean");
			break;
		case "byte":
			return Type.GetType ("System.Byte");
			break;
		case "sbyte":
			return Type.GetType ("System.SByte");
			break;
		case "char":
			return Type.GetType ("System.Char");
			break;
		case "decimal":
			return Type.GetType ("System.Decimal");
			break;
		case "double":
			return Type.GetType ("System.Double");
			break;
		case "float":
			return Type.GetType ("System.Float");
			break;
		case "int":
			return Type.GetType ("System.Int32");
			break;
		case "uint":
			return Type.GetType ("System.UInt32");
			break;
		case "long":
			return Type.GetType ("System.Int64");
			break;
		case "ulong":
			return Type.GetType ("System.UInt64");
			break;
		case "object":
			return Type.GetType ("System.Object");
			break;
		case "short":
			return Type.GetType ("System.Int16");
			break;
		case "ushort":
			return Type.GetType ("System.UInt16");
			break;
		case "string":
			return Type.GetType ("System.String");
			break;
		default:
			return Type.GetType (s);
			break;
		
		}
	}
}

