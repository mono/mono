'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module F
	Function fun() As Integer
		fun = 10 
	End Function
	Sub Main()	
		Dim i as Integer = fun()
		if I<>10 then
			Throw new System.Exception("Local Variables not working properly. Expected 10 but got"&i)
		End if
	End Sub
End Module
