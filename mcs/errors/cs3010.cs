// cs3010.cs: 'I.Error': CLS-compliant interfaces must have CLS-compliant members
// Line: 12

using System;
[assembly:CLSCompliant (true)]

public interface I {
        [CLSCompliant (true)]
        void Valid (bool arg);
    
        [CLSCompliant (false)]
        event AssemblyLoadEventHandler Error;
}