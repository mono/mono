'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class Test1234567890
	 Sub fun(ByRef a as Char, Byref a1 as String)	
	 End Sub
	 Sub fun(ByRef a as integer, Byref a1 as Long)	
	 End Sub
End Class

<TestFixture>_
Public Class TypeMembers9
	_<Test, ExpectedException (GetType (System.Reflection.AmbiguousMatchException))>
        Public Sub TestForException()
		      dim o as Object = new Test1234567890()
			o.fun(Nothing, Nothing)  		
        End Sub
End Class 
