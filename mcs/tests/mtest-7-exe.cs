// Compiler options: -addmodule:mtest-7-dll.netmodule

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
