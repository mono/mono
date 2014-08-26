// CS7003: Unbound generic name is not valid in this context
// Line: 10

class G<T>
{
}

class C
{
	const string f = nameof (G<>);
}