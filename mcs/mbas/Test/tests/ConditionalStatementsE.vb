'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Sub Main()	
	Dim i as integer = 0
	if False then 
		i = 1
	Elseif False
		i = 2
	Else 
		if False then
			i = 3
		else
			i = 4
		End if
	End if
	if i<>4 then
		Throw New System.Exception("If else if not working properly. Expected 4 but got "&i)
	End if
    End Sub
End Module

