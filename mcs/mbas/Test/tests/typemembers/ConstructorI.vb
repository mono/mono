'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Imports A

NameSpace A
	Public Class B
		Shared Public i as integer
	End Class
End Namespace

Class AB
	Shared Sub New()				
		A.B.i = A.B.i + 1
	End Sub
End Class

Module Test
    Public Sub Main()
		Dim a1 as AB = new AB()
		Dim b2 as AB = new AB()
		Dim c3 as AB = new AB()
		if A.B.i<>1
			Throw new System.Exception("Shared Constructor not working")
		End if
    End Sub
End Module

