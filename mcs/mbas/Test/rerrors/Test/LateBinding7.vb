'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class Test12345678
	 Sub fun(ByRef a as Integer, Byref a1 as Date)	
	 End Sub
End Class

<TestFixture>_
Public Class TypeMembers7
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()
		   dim a1 as long = 10
		   dim a as long = 10
		   dim o as Object = new Test12345678()
		   o.fun(a,a1)              		
        End Sub
End Class 
