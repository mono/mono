'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.OverflowException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ExpConversionStringtoByteC
	_<Test, ExpectedException (GetType (System.OverflowException))>
        Public Sub TestForException()		
			Dim a as Byte
			Dim b as String= "256"
			a = CByte(b)
        End Sub
End Class 