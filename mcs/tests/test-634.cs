public class Test
{
   static public void TestFunc ()
   {
       return;

       string testStr;

       System.AppDomain.CurrentDomain.AssemblyLoad += delegate
(object Sender, System.AssemblyLoadEventArgs e)
       {
           testStr = "sss";
       };
   }

   public static void Main (string[] args)
   {
       TestFunc ();
   }
}
