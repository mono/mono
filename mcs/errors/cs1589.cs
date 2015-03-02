// CS1589: Unable to include XML fragment `/root/@attr' of file `cs1589.inc'. The specified node cannot be inserted as the valid child of this node, because the specified node is the wrong type
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

