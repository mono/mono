// CS1961: The covariant type parameter `V' must be invariantly valid on `Both<U,V>(Covariant<None<Contra<V>>>)'
// Line: 8

delegate T Covariant<out T> ();
delegate void Contra<in T> (T t);
delegate void None<T> (T t);

delegate void Both<in U, out V> (Covariant<None<Contra<V>>> b);
