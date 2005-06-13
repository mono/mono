// cs0136-3.cs: A local variable named `y' cannot be declared in this scope because it would give a different meaning to `y', which is already used in a `parent or current' scope to denote something else
// Line: 8

class X
{
   static void y () { }
   static void Main () {
     y ();
     int y = 5;
   }
}
