'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class Variables2
	_<Test, ExpectedException (GetType (System.InvalidCastException))>
        Public Sub TestForException()
		dim a as string = "hello"
		dim i(1,a) as A 
        End Sub
End Class 
