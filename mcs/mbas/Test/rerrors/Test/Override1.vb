'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

' System.MissingMemberException

Imports System
Imports Nunit.Framework


Class Inheritance123456        
        Overridable Function fun(j as Integer)
	  End Function
End Class

Class Inheritance12345
	Inherits Inheritance123456  
	Overrides Function fun(j as Integer)	  
			i=j
			return i
	 End Function
	 Dim i as Integer
End Class

<TestFixture>_
Public Class InheritanceM
	_<Test, ExpectedException (GetType (System.MissingMemberException))>
        Public Sub TestForException()	
		Dim a as Object = new Inheritance12345()
		a.fun(a.i)
        End Sub
End Class 
