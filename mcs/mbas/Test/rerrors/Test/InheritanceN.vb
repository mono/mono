'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class AQ
	Public a as Integer
End Class

Class PQ
	Inherits AQ
	Public i as Integer
End Class

Class PQ1
	Inherits PQ
	Public b as Byte
End Class

<TestFixture> _
public Class InheritanceN
	<Test, ExpectedException (GetType (System.InvalidCastException))> _
	Sub Main()
		Dim a as AQ = new AQ()
		Dim a1 as PQ1 = new PQ1()
		a1 = a
	End Sub
End Class



