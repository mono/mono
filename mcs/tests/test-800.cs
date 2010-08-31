// Compiler options: -unsafe

struct S
{
}

unsafe class C
{
	const int***[] m_p = null;
	const S**[,] m_s2 = null;
	
	public static void Main ()
	{
		const int*[] c = null;
		const S*[,] s = null;
		const void*[][][][] v = null;
			
		const int***[] c2 = m_p;
		const S**[,] s2 = m_s2;
		const void**[][] v2 = null;
		
		int***[] a1 = m_p;
		S**[,] a2 = m_s2;
	}
}
