'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 17
REM ExpectedError: BC32006
REM ErrorMessage:'Char' values cannot be converted to 'Integer'.

Imports System

Class A
End Class

Module Test
    Public Sub Main()
		Dim a as integer = new Char
    End Sub
End Module
