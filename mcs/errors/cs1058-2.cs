// CS1058: A previous catch clause already catches all exceptions. All non-exceptions thrown will be wrapped in a `System.Runtime.CompilerServices.RuntimeWrappedException'
// Line: 15
// Compiler options: -warnaserror -warn:4

using System.Runtime.CompilerServices;

[assembly: RuntimeCompatibility (WrapNonExceptionThrows=true)]

class C
{
   static void Main() 
   {
      try {}
      catch (System.Exception) { }
      catch {}
   }
}
