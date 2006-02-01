// Compiler options: support-test-353.cs -out:test-353.exe

using System;
 
[Obsolete]
public class One { }
 
#pragma warning disable 612
 
public class Two {
        private One one;
}
