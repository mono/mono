'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError:  BC30081
REM ErrorMessage: 'If' must end with a matching 'End If'.

Module Test
    Sub Main()	
	Dim i as integer = 1000
	if i then 
		i = 4
	else
		i = 1
	if i<>4 then
		Throw New System.Exception("If else if not working properly. Expected 4 but got "&i)
	End if
    End Sub
End Module

