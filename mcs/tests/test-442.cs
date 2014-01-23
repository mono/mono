// Compiler options: -unsafe

using System;

namespace ConsoleApplication1 {
   class Program {
       unsafe public static void Main(string[] args) {
           int[] i = new int[] { 10 };
           fixed (int* p = i) {
               int*[] q = new int*[] { p };
               *q[0] = 5;
               Console.WriteLine(*q[0]);
           }
       }
   }
}

