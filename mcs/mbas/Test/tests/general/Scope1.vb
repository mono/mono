'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Unhandled Exception: System.InvalidCastException: Cast from string "Hello" to type 'Integer' is not valid. 

Imports System
Class A
	Shared Sub fun(i as Integer)
		System.Console.WriteLine("Outer Integer {0}",i)
	End Sub
	Shared Sub fun(i as String)
		System.Console.WriteLine("Outer String {0}",i)
	End Sub
	Class AB
		Sub gun()
		 Try
			fun(1)
			fun("Hello")
			Catch e As Exception
		         System.Console.WriteLine(e.Message)
		 End Try
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
