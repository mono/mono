// CS0455: Type parameter `T3' inherits conflicting constraints `X' and `Y'
// Line: 15

class X
{
}

class Y
{
}

class C<T1, T2, T3, T4>
	where T1 : X
	where T2 : T1
	where T3 : Y, T4
	where T4 : T2
{
}