'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.OverflowException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ExpConversionDoubletoDecimalB
	_<Test, ExpectedException (GetType (System.OverflowException))>
        Public Sub TestForException()	
			Dim a as Decimal
			Dim b as Double = Double.PositiveInfinity
			a = CDec(b)
        End Sub
End Class 

