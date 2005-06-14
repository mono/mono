// Compiler options: -addmodule:test-416-mod.netmodule

using n1;
using System;

public class ModTest
{
        
        public static void Main(string[] args)
        {
                Adder a=new Adder();
                Console.WriteLine(a.Add(2,3));
        }

}
