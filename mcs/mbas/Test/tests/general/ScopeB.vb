'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'To check if the Inner class is accessed or the outer class is accessed... Inner class is accessed in this case 

Class A
	Shared Sub fun(i as Integer)
		throw new System.Exception("#A1 Outer Integer")
	End Sub
	Shared Sub fun(i as String)
		throw new System.Exception("#A2 Outer String")
	End Sub
	Class AB
		Sub gun()
			fun(1)
			fun(2)
		End Sub
		Shared Sub fun(i as Integer)
			'System.Console.WriteLine("Inner class Integer {0}",i)
		End Sub
	End Class
End Class

Module ScopeA
	Sub Main()
		Dim a as A.AB=new A.AB()
		a.gun()
	End Sub
End Module
