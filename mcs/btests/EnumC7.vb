Imports System

Class C
	private Enum E
		q = 10
		m = 20
	End Enum

End Class

Module M
	Sub Main()
		dim x as C = new C ()
		Console.WriteLine(c.E.m)
	End Sub
End Module
