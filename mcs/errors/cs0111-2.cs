// cs0111.cs: Class 'ISample' already defines a member called 'set_Item' with the same parameter types
// Line: 6

public interface ISample {
        void set_Item (int a, int b);
        int this[int i] { set; }
}