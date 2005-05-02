'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30070
REM ErrorMessage: Next control variable does not match For loop control variable 'j'.

Module Test
    Sub Main()
		For i as integer = 2 to 4
			For j as integer = 5 to 6
			Next i
		Next j
    End Sub
End Module
