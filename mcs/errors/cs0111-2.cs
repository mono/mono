// cs0111.cs: Identifier 'CLSClass.vAluE' differing only in case is not CLS-compliant
// Line: 8

[assembly:System.CLSCompliant(true)]

public interface ISample {
        void set_Item (int a, int b);
        int this[int i] { set; }
}