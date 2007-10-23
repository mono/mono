// CS0082: A member `Class.op_Implicit(byte)' is already reserved
// Line: 9

public class Class {
        static public implicit operator Class(byte value) {
               return new Class();
        }
    
        public static void op_Implicit (byte value) {}
}
