'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class TestTypeMembers13
	Public Function fun(i as Decimal, Optional a1 as Char = "c") As Integer
	End Function
	Public Function fun(i as Long, a1 as Char) As Integer
	End Function
End Class


<TestFixture>_
Public Class TypeMembers13
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()	
                Dim o as Object=new TestTypeMembers13()
		    dim a as integer = o.fun(i := 2, a1 := "c")
        End Sub
End Class 
