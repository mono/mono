// cs0111-3.cs: `Class.implicit operator Class(byte)' is already defined. Rename this member or use different parameter types
// Line: 9

public class Class {
        static public implicit operator Class(byte value) {
               return new Class();
        }
    
        public static void op_Implicit (byte value) {}
}