Imports System

Structure S
	Dim a as String
	Const b as integer = 25

	Sub NEW(l as long)
	End Sub
End Structure


Module M
	Sub Main()
		dim x as S = new S(100)
	End Sub
End Module
