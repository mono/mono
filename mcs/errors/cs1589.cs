// Compiler options: -doc:xml-025.xml -warn:1 -warnaserror

namespace Testing
{
   /// <include file='cs1589.inc' path='/foo' />
   public class Test
   {
	public static void Main ()
	{
	}

	/// <include file='cs1589.inc' path='/root/@attr'/>
	public string S3;
   }
}

