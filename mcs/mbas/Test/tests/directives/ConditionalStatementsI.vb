'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Sub Main()	
	Dim i as integer = 10
        Select Case i
		Case 10.5
			i = 15
		Case 20.5
			i = 20
		Case 30.5
			i = 30
        End Select
	if i<>15 then
		Throw New System.Exception("Select not working properly. Expected 15 but got "&i)
	End if
    End Sub
End Module

