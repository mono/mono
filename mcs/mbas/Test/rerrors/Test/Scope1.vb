'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.InvalidCastException

Imports System
Imports Nunit.Framework

Class Scope1234
	Shared Sub fun(i as Integer)
		System.Console.WriteLine("Outer Integer {0}",i)
	End Sub
	Shared Sub fun(i as String)
		System.Console.WriteLine("Outer String {0}",i)
	End Sub
	Class AB
		Sub gun()		
			fun(1)
			fun("Hello")		
		End Sub
		Shared Sub fun(i as Integer)
			System.Console.WriteLine("Inner class Integer {0}",i)
		End Sub
	End Class
End Class

<TestFixture>_
Public Class Scope123
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()	
		Dim a as Scope1234.AB=new Scope1234.AB()
		a.gun()
        End Sub
End Class 
