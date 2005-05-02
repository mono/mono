'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

' System.InvalidCastException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ImpConversionStringtoLongB
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
			Dim a as Long
			Dim b as String= "Program"
			a = b
        End Sub
End Class 
