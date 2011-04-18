// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `Splay<T>.Node'
// Line: 10
// Compiler options: /unsafe
// Similar code to #75772, but without field of type T.

public class Splay<T>
{
  unsafe private struct Node
  {
    private Node *            parent;
  } 
}

