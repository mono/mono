'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'To check if the Inner class is accessed or the outer class is accessed... Inner class is accessed in this case 

Class A
	Shared Sub fun(i as Integer)
		System.Console.WriteLine("Outer Integer {0}",i)
	End Sub
	Shared Sub fun(i as String)
		System.Console.WriteLine("Outer String {0}",i)
	End Sub
	Class AB
		Sub gun()
			fun(1)
			fun(2)
		End Sub
		Shared Sub fun(i as Integer)
			System.Console.WriteLine("Inner class Integer {0}",i)
		End Sub
	End Class
End Class

Module ScopeA
	Sub Main()
		Dim a as A.AB=new A.AB()
		a.gun()
	End Sub
End Module
