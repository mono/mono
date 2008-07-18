// CS0841: A local variable `y' cannot be used before it is declared
// Line: 8

class X
{
   static void y () { }
   static void Main () {
     y ();
     int y = 5;
   }
}
