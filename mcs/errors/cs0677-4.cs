// CS0677: `C<T>.t': A volatile field cannot be of the type `T'
// Line: 8

public class C<T>  where T : struct
{
	volatile T t;
}
