'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 16
REM ExpectedError: BC32006
REM ErrorMessage: 'Char' values cannot be converted to 'Integer'. Use Microsoft.VisualBasic.AscW' to interpret a character as a Unicode value or 'Microsoft.VisualBasic.Val' to interpret it as a digit.

Imports System

Module Default1	
	Sub Main()
		Dim a As Integer
		Dim c as char
		a=c
	End Sub
End Module
