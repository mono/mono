'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Sub Main()	
	Dim i as integer = 5
        Select Case i
            Case 5
 			i = 10		
            Case 10
			i = 20
	      Case 20
			i = 30
		Case 30
			i = 5
		Case Else 
			i = 6
        End Select
	if i<>10 then
		Throw New System.Exception("Select not working properly. Expected 10 but got "&i)
	End if
    End Sub
End Module

