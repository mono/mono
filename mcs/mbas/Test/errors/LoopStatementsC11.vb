'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30337
REM ErrorMessage: 'For' loop control variable cannot be of type 'Date'.

Module Test
    Sub Main()
		For i as Date
		Next
    End Sub
End Module
