// CS0120: An object reference is required to access non-static member `Test.Add8(int)'
// Line: 12

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

public class Test {

        public Test () : this (Add8(4), 6) {
                string hostName = System.Net.Dns.GetHostName ();
                Console.WriteLine ("Hostname: " + hostName);
        }

        public Test (int i, int j) {
                Console.WriteLine ("GOT : " + i + " : " + j);
        }


        public static void Main (String[] args) {
                Test t = new Test ();
        }

        private int Add8 (int i) {
                return i + 8;
        }

}
