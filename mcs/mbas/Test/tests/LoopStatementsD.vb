'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
    Sub Main()
		Dim ii as Integer = 0
		Try
		For i as Byte = 2 to 4 step "hello"
			For j as integer = 5 to 6
			Next 
		Next 
		Catch e as System.Exception
			ii = 1
		End Try
		if ii<>1 then
			Throw new System.Exception("For loop not working properly")
		End if 	
    End Sub
End Module
