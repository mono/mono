'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class Test12345
	  Sub fun(ByRef a as date)	
	  End Sub
End Class

<TestFixture>_
Public Class TypeMembers4
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()
		   Dim a as integer = 10	
		   Dim o as Object = new Test12345()	
		   o.fun(a)               		
        End Sub
End Class 
