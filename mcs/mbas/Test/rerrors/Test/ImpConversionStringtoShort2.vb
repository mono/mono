'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

' System.InvalidCastException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ImpConversionStringtoShortB
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
                Public Sub TestMismatchException ()
			Dim a as Short
			Dim b as String= "Program"
			a = b
        End Sub
End Class 
