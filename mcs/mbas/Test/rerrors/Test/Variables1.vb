'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class VariablesI
	_<Test, ExpectedException (GetType (System.TypeInitializationException))>
	  Readonly Shared Public i as integer = "Hello"
        Public Sub TestForException()
		if i <> 1
		End if		
        End Sub
End Class 
