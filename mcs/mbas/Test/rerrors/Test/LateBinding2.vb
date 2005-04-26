'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class Test1234
	 Function fun() As Date
		return "1234"			 
	 End Function
End Class

<TestFixture>_
Public Class TypeMembers2
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()
		   dim a as Date
		   dim o as Object = new Test1234()
		   a = o.fun()               		
        End Sub
End Class 
