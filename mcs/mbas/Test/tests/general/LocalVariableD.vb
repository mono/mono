'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module M
	Function fun()
		Static Dim y as Integer = 10
		y = y + 1 
		return y
	end Function
	Function fun1()
		return fun()		
	end Function
      Sub Main()		
		Dim x as Integer
		fun()
		x = fun1()
		if x <> 12
			Throw new System.Exception("Static declaration not implemented properly. Expected 12 but got " &x)
		End if
      End Sub
End Module
