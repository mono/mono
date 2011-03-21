// CS1961: The contravariant type parameter `T' must be invariantly valid on `InterfaceContravariat<T>.Prop'
// Line: 4

interface InterfaceContravariat<in T>
{
	T Prop { set; get; }
}
