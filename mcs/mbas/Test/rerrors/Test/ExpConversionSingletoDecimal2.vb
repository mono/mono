'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.OverflowException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ExpConversionSingletoDecimalB
	_<Test, ExpectedException (GetType (System.OverflowException))>
        Public Sub TestForException()		
			Dim a as Decimal
			Dim b as Single = Single.PositiveInfinity
			a = CDec(b)
        End Sub
End Class 
