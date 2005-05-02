'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 20
REM ExpectedError: BC30390
REM ErrorMessage:  'AB.Private Sub New()' is not accessible in this context because it is 'Private'.

Imports System

Class AB
	Private Sub New()		
		Throw new System.Exception("Constructor not working properly")
	End Sub
End Class

Module Test
    Public Sub Main()
		Dim a as AB = new AB()
    End Sub
End Module

