// cs0111.cs: Class 'Class' already defines a member called 'op_Implicit' with the same parameter types
// Line: 9

public class Class {
        static public implicit operator Class(byte value) {
               return new Class();
        }
    
        public static void op_Implicit (byte value) {}
}