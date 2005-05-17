'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class NamedArgumentClass
	Public Function fun(Byval i as integer, Byval a1 as Integer , Optional Byval j as Integer=30) As Integer
		if a1="c" and i=2 and j=30
			return 10
		End if
		return 11	
	End Function
End Class

<TestFixture>_
Public Class NamedArguments1
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
    Public Sub TestForException()
		   dim o as Object = new NamedArgumentClass()
		   dim c as char = "c"
		   dim a as integer = o.fun(i := 2.321, a1 := c)
     End Sub
End Class 
