'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class Test123
	 Function fun() As Object
		return "c"			 
	 End Function
End Class

<TestFixture>_
Public Class TypeMembers1
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()
		   dim a as Long
		   dim o as Object = new Test123()
		   a = o.fun()               		
        End Sub
End Class 
