'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError:  BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Integer' to 'Boolean'.

Option Strict on
Module Test
    Sub Main()	
	Dim i as integer = 1000
	if i then 
		i = 4
	else
		i = 1
	End if
	if i<>4 then
		Throw New System.Exception("If else if not working properly. Expected 4 but got "&i)
	End if
    End Sub
End Module

