'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class Test1234567
	 Sub fun(ByRef a as Long, Byref a1 as Integer)	
		   a = a + 10
		   a1 = a1 + 20
	 End Sub
	 Sub fun(ByRef a as Integer, Byref a1 as Long)	
		   a = a + 20
		   a1 = a1 + 10
	 End Sub
End Class

<TestFixture>_
Public Class TypeMembers6
	_<Test, ExpectedException (GetType (System.Reflection.AmbiguousMatchException))>
        Public Sub TestForException()
		   dim a1 as long = 10
		   dim a as long = 10
		   dim o as Object = new Test1234567()
		   o.fun(a,a1)               				 
        End Sub
End Class 
