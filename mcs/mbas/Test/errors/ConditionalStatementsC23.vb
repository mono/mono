'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError:  BC30087
REM ErrorMessage: 'End If' must be preceded by a matching 'If'.

Module Test
    Sub Main()	
	Dim i as integer = 1000
	i = 4
	End if
	if i<>4 then
		Throw New System.Exception("If else if not working properly. Expected 4 but got "&i)
	End if
    End Sub
End Module

