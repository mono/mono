'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError:   BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Double' to 'Integer'.

Option Strict on
Module Test
    Sub Main()	
	Dim i as integer = 10
        Select Case i
		Case 10.5
			i = 15		
        End Select
	if i<>15 then
		Throw New System.Exception("Select not working properly. Expected 15 but got "&i)
	End if
    End Sub
End Module

