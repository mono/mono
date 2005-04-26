'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class Test123456
	  Sub fun(ByRef a as Char)	
	  End Sub
End Class

<TestFixture>_
Public Class TypeMembers5
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()
		   Dim a as integer = 10	
		   Dim o as Object = new Test123456()	
		   o.fun(a)               		
        End Sub
End Class 
