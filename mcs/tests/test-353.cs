// Compiler options: test-353-2.cs

using System;
 
[Obsolete]
public class One { }
 
#pragma warning disable 612
 
public class Two {
        private One one;
}
