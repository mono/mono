'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

' System.InvalidCastException

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class ExpConversionStringtoLongB
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()		
			Dim a as Long
			Dim b as String= "Program"
			a = CLng(b)
        End Sub
End Class 