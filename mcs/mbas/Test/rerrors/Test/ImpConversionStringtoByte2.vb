'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

' System.InvalidCastException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ImpConversionStringtoByteB
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()	
			Dim a as Byte
			Dim b as String= "Program"
			a = b
        End Sub
End Class 