'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError:  BC30058
REM ErrorMessage: Statements and labels are not valid between 'Select Case' and first 'Case'.

Module Test
    Sub Main()	
	Dim i as integer = 10
        Select Case i
		        Select Case i
			  End Select
        End Select
	if i<>10 then
		Throw New System.Exception("Select not working properly. Expected 4 but got "&i)
	End if
    End Sub
End Module

