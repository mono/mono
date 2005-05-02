'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.InvalidCastException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ExpConversionStringtoDoubleB
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()		
			Dim a as Double
			Dim b as String= "Program"
			a = CDbl(b)
        End Sub
End Class 