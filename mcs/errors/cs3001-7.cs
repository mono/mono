// cs3001-4.cs: Identifier 'CLSClass.vAluE' differing only in case is not CLS-compliant
// Line: 8

using System;
[assembly:CLSCompliant(true)]

[CLSCompliant(false)]
public interface I {
        [CLSCompliant(false)]
        void Foo();

        [CLSCompliant(true)]
        long this[uint indexA] { set; }
}
