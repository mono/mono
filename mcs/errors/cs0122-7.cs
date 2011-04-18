// CS0122: `BB.AnEvent' is inaccessible due to its protection level
// Line: 10

using System;

class X 
{
       static void Main ()
       {
               BB b = new BB ();
               b.AnEvent += DoIt;
       }
       
       public static void DoIt (object sender, EventArgs args) {}
}

public class BB
{
       event EventHandler AnEvent;
}

