'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.MissingMemberException

Imports System
Imports Nunit.Framework

Class Inheritance123
   	  Private Sub fun()
	  End Sub
End Class

Class Inheritance1234
        Inherits Inheritance123
	  Public Sub G()
        End Sub
End Class

<TestFixture>_
Public Class Mismatch123
	_<Test, ExpectedException (GetType (System.MissingMemberException))>
        Public Sub TestForException()	
		Dim a As Object
		a=New Inheritance123()
		a.fun()
		a=New Inheritance1234()
		a.fun()
        End Sub
End Class 
