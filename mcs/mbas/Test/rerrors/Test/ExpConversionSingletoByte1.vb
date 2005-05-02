'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.OverflowException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ExpConversionSingletoByteA
	_<Test, ExpectedException (GetType (System.OverflowException))>
        Public Sub TestForException()		
		Dim a as Byte 
		Dim b as Single = 3000000000
		a = CByte(b)
        End Sub
End Class 
