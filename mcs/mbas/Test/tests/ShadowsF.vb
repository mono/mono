'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Class A
	Sub fun()
	End Sub
End Class

Class AB
	Inherits A
	Shadows Sub fun()
	End Sub
End Class

Module ShadowE
	Sub Main()
		Dim a as AB=New AB()
		a.fun()
		CType(a, A).fun()
	End Sub
End Module
