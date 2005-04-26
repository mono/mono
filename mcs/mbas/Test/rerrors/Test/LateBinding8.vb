'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class Test123456789
	 Sub fun(ByRef a() as Long)	
	 End Sub
End Class

<TestFixture>_
Public Class TypeMembers8
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()
		   dim i() as Integer = {1,2,3}
		   dim o as Object = new Test123456789()
		   o.fun(i)   'Constant value passed          		
        End Sub
End Class 
