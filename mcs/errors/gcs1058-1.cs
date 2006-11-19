// cs1058.cs: A previous catch clause already catches all exceptions. All non-exceptions thrown will be wrapped in a `System.Runtime.CompilerServices.RuntimeWrappedException'
// Line: 11
// Compiler options: -warnaserror -warn:4

class C
{
   static void Main() 
   {
      try {}
      catch (System.Exception) { }
      catch {}
   }
}
 
