'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module InterfaceA
	Interface A
		Sub fun()		
		Sub fun1()
		Sub fun2()
		Sub fun3()
	End Interface
	
	Class B		
		Implements A
		Public Sub AA1() Implements A.fun
		End Sub
		Private Sub AA2() Implements A.fun1
		End Sub
		Protected Sub AA3() Implements A.fun2
		End Sub
		Sub AA4() Implements A.fun3
		End Sub
	End Class
	
	Sub Main()		
	End Sub
End Module
 
