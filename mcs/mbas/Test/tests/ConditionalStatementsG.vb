'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'To prove "Else if" and "Elseif" are same

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

