// cs0208-3.cs : Cannot take the address of, get the size of, or declare a pointer to a managed type `Splay<T>.Node'
// Line: 10
// Compiler options: /unsafe
// Similar code to #75772, but without field of type T.
// Compiler options: /unsafe
public class Splay<T>
{
  unsafe private struct Node
  {
    private Node *            left, right;
    private Node *            parent;
//    private T                 data;

    public static void Main () {}
  } 
}

