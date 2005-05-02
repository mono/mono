'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Cheks if the outer class members can be accessed using an object reference or not... of course works...
Class A
	Sub fun(i as Integer)
		if i<>1
			throw new System.Exception("#A1 Outer Integer")
		End if
	End Sub
	Sub fun(i as String)
		if i<>"Hello"
			throw new System.Exception("#A2 Outer String ")
		End if
	End Sub
	Class AB
		Sub gun()
			Dim a as A=new A()
			a.fun(1)
			a.fun("Hello")
		End Sub		
	End Class
End Class

Module ScopeA
	Sub Main()
		Dim a as A.AB=new A.AB()
		a.gun()
	End Sub
End Module
