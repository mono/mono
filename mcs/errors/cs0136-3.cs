// cs0136: 'y' has a different meaning later in the block
// Line: 8

class X
{
   static void y () { }
   static void Main () {
     y ();
     int y = 5;
   }
}
