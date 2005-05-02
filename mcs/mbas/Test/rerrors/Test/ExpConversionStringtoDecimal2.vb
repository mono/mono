'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.InvalidCastException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ExpConversionStringtoDecimalB
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()		
			Dim a as Decimal
			Dim b as String= "Program"
			a = CDec(b)
        End Sub
End Class 