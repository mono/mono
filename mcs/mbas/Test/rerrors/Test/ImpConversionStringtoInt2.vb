'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

' System.InvalidCastException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ImpConversionStringtoIntegerB
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
			Dim a as Integer
			Dim b as String= "Program"
			a = b
        End Sub
End Class 

