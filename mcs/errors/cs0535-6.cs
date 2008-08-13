// CS0535: `CA' does not implement interface member `IA.M()'
// Line: 14

interface IA
{
	void M ();
}

interface IB
{
	void M ();
}

class CA : IA, IB
{
	void IB.M ()
	{
	}
}
