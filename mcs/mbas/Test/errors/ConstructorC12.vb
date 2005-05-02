'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30043 
REM ErrorMessage: 'Me' is valid only within an instance method.

Imports System

Class AB
	Shared Sub New()				
		Me.New(10)
	End Sub
End Class

Module Test
    Public Sub Main()
		Dim a as AB = new AB()
    End Sub
End Module

