// CS1540: Cannot access protected member `A.member' via a qualifier of type `A'. The qualifier must be of type `B' or derived from it
// Line: 17
// NOTE: csc report simple inaccessible error which is less precise

using System;

class A
{
       protected event EventHandler member;
}

class B : A
{
       static void Main ()
       {
               A a = new A ();
               a.member += Handler;
       }
       
       static void Handler (object sender, EventArgs args) {}
}
