'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Interface A
	Function A1()
End Interface

Class B
	Implements A
	Public Function A1() implements A.A1
	End Function
End Class

Class C
	Inherits B
End Class

<TestFixture> _
public Class Interface1
	<Test, ExpectedException (GetType (System.InvalidCastException))> _
	Public Sub TestForException()
		Dim x as A = new B()
		Dim x1 as C= new C()
		x = x1
	End Sub
End Class
