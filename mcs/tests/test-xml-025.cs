// Compiler options: -doc:xml-025.xml dlls/test-xml-025-relative.cs

namespace Testing
{
   /// <include file='test-xml-025.inc' path='/foo' />
   public class Test
   {
	public static void Main ()
	{
	}

	/// <include file='test-xml-025.inc' path='/root'/>
	public string S1;

	/// <include file='test-xml-025.inc' path='/root/child'/>
	public string S2;

	/// <include file='test-xml-025.inc' path='/root/@attr'/>
	public string S3;
   }
}

