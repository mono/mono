// cs1575.cs: A stackalloc expression requires [] after type
// Line: 9
// Compiler options: -unsafe

class E
{
   public unsafe void Method (int i)
   {
       long* p = stackalloc long; 
   }
}
