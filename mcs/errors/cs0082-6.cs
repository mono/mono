// CS0082: A member `ISample.set_Item(int, int)' is already reserved
// Line: 6

public interface ISample {
        void set_Item (int a, int b);
        int this[int i] { set; }
}
