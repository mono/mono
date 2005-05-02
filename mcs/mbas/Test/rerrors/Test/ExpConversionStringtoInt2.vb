'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.InvalidCastException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ExpConversionStringtoIntegerB
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()		
			Dim a as Integer
			Dim b as String= "Program"
			a = CInt(b)
        End Sub
End Class 