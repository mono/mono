'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Sub Main()
		Dim i 
		For i = 9 to 0 step 1			
		Next 
		if i<>9 then
			throw new System.Exception("For loop not working. Expected 9 but got " &i)
		End if
    End Sub
End Module
