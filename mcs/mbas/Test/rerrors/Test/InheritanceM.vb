'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class P
	Public i as Integer
End Class

Class P1
	Inherits P
	Public b as Byte
End Class

<TestFixture> _
Public Class InheritanceM
	<Test, ExpectedException (GetType (System.InvalidCastException))> _
	Sub Main()		
		Dim a as P = new P()
		Dim a1 as P1 = new P1()
		a1 = a
	End Sub
End Class



