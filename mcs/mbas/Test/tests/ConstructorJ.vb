'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Checking circular references

Imports System

Class A
    Public Shared X As Integer = B.Y + 1
    Shared Sub Hello()
    		if A.X <> 2 or B.Y <> 1
			Throw new System.Exception("Shared Construtor not working")
		End if
    End Sub
End Class

Class B
    Public Shared Y As Integer = A.X + 1
End Class


Module Test
    Public Sub Main()		
		A.Hello()
    End Sub
End Module

