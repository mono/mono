//
// This test excercises user defined conversions and an implicit
// conversion to a type afterwards.
//
// 
using System;

class Blah {

 public static int Main ()
 {
  Blah k = new Blah ();

  float f = k;

  if (f == 2){
   Console.WriteLine ("Best implicit operator selected correctly");
   return 0;
  }
  return 1;

 }

 public static implicit operator byte (Blah i)
 {
  Console.WriteLine ("Blah->byte");
  return 0;
 }


 public static implicit operator short (Blah i)
 {
  Console.WriteLine ("Blah->short");
  return 1;
 }

 public static implicit operator int (Blah i)
 {
  Console.WriteLine ("Blah->int");
  return 2;
 }


}

