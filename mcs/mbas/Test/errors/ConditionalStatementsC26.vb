'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

Module Test
    Sub Main()	
	Dim i as integer = 10
        Select Case 
		Case 20-10.5
			i = 15
        End Select
	if i<>15 then
		Throw New System.Exception("Select not working properly. Expected 15 but got "&i)
	End if
    End Sub
End Module

