// cs0208-2.cs : Cannot take the address of, get the size of, or declare a pointer to a managed type `Splay<T>.Node'
// Line: 9
// Compiler options: /unsafe

public class Splay<T>
{
  unsafe private struct Node
  {
    private Node *            parent;
    private T                 data;
  } 
}

