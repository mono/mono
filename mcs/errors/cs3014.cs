// cs3014.cs: 'I.Valid(bool)' cannot be marked as CLS compliant because the assembly is not marked as compliant
// Line: 8

using System;

public interface I {
        [CLSCompliant (true)]
        void Valid (bool arg);
}