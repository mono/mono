// cs0111-2.cs: `ISample.set_Item(int, int)' is already defined. Rename this member or use different parameter types
// Line: 6

public interface ISample {
        void set_Item (int a, int b);
        int this[int i] { set; }
}
