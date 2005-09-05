// cs0208-4.cs : Cannot take the address of, get the size of, or declare a pointer to a managed type `Splay<T>.Node'
// Line: 10
// Compiler options: /unsafe
// similar one to #75772 but using a class instead of a struct.
// Compiler options: /unsafe
public class Splay<T>
{
  unsafe private class Node
  {
    private Node *            left, right;
    private Node *            parent;
    private T                 data;
  } 
}

