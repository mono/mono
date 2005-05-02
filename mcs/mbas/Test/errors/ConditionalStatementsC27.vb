'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30311
REM ErrorMessage: Value of type 'Integer' cannot be converted to 'Date'.

Module Test
    Sub Main()	
	Dim i as Date
        Select Case i 
		Case 20
        End Select
    End Sub
End Module

