// Compiler options: -r:System.Xml.dll

abstract class MethodWrapper
{
	private string[] declaredExceptions;

	internal void SetDeclaredExceptions(MapXml.Throws[] throws)
	{
		if(throws != null)
		{
			declaredExceptions = new string[throws.Length];
			for(int i = 0; i < throws.Length; i++)
			{
				declaredExceptions[i] = throws[i].Class;
			}
		}
	}
}

namespace MapXml {

    using System;
    using System.Xml.Serialization;
    
    public class Throws
    {
        [XmlAttribute("class")]
        public string Class;
            
        public static void Main ()
        {
        }
    }
}
