// CS3010: `I.Error': CLS-compliant interfaces must have only CLS-compliant members
// Line: 13
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

public interface I {
        [CLSCompliant (true)]
        void Valid (bool arg);
    
        [CLSCompliant (false)]
        event AssemblyLoadEventHandler Error;
}
