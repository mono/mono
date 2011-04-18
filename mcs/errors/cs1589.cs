// CS1589: Unable to include XML fragment `/root/@attr' of file `cs1589.inc' (Cannot insert specified type of node as a child of this node.)
// Line: 15
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

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

