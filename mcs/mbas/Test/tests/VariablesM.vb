'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Class AA
	Inherits System.MarshalByRefObject
	Public Function fun()
	End Function
End Class


Class AAA
	Public Function fun(a As AA)		
	End Function
End Class

Module Test
    Public Sub Main()
		dim b as AA = new AA()
		dim a as AAA = new AAA()
		a.fun(b)
    End Sub
End Module

