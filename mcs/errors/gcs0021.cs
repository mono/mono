// CS0021: Cannot apply indexing with [] to an expression of type `G'
// Line: 5
public class Foo<G> {
    public static void Bar() {
        int i = default(G)[0];
    }
}

