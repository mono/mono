'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Class A
	Public i as Integer
	Sub New()
		i = 20
	End Sub
	Sub New (a as A)
		i = a.i
	End Sub
End Class

Module Test
    Public Sub Main()
		dim a as A = new A()
		dim j as A = new A(a)
		if j.i<>20
			Throw new System.Exception("Initializer not working")
		End if
    End Sub
End Module
