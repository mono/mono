'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'To prove "Else if" and "Elseif" are same

Module Test
    Sub Main()	
	Dim i as integer = 0
	if False then 
		i = 1
	Elseif False then
		i = 2
	Else if False then
		i = 3
	else
		i = 4
	End if
	if i<>4 then
		Throw New System.Exception("If else if not working properly. Expected 4 but got "&i)
	End if
    End Sub
End Module

