using System;
using System.IO;

namespace testit
{
  [Serializable] public class SPoint
    {
      public Double x = 1;
      public Double y = 2;
      public SPoint () {;}
    }
  class Class1
    {

      static void serialize()
	{
	  System.Runtime.Serialization.IFormatter xx = 
	      new System.Runtime.Serialization.Formatters.Soap.SoapFormatter ();
	  FileStream _out = new FileStream ("out.xml", FileMode.Create, FileAccess.Write, FileShare.None);
	  xx.Serialize (_out, new SPoint());
	}
      static void deserialize()
	{
	  System.Runtime.Serialization.IFormatter xx = 
	      new System.Runtime.Serialization.Formatters.Soap.SoapFormatter ();
	  FileStream _out = new FileStream ("out.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
	  SPoint ob = xx.Deserialize (_out) as SPoint;
	  Console.WriteLine (ob.x);
	}
      public static void Main (String [] args)
	{
	  if (args.Length > 0)
	    deserialize();
	  else
	    serialize();
	}
    }
}
