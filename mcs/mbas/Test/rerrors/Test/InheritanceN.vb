'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class A
	Public a as Integer
End Class

Class P
	Inherits A
	Public i as Integer
End Class

Class P1
	Inherits P
	Public b as Byte
End Class

<TestFixture> _
public Class InheritanceN
	<Test, ExpectedException (GetType (System.InvalidCastException))> _
	Sub Main()
		Dim a as A = new A()
		Dim a1 as P1 = new P1()
		a1 = a
	End Sub
End Class



