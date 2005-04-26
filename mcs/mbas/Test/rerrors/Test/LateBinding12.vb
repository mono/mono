'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class TestTypeMembers10
	Private fun as Integer = 10
End Class

Class TestTypeMembers101
	Public fun as Integer = 20
End Class

<TestFixture>_
Public Class TypeMembers10
	_<Test, ExpectedException (GetType (System.MissingMemberException))>
        Public Sub TestForException()
		   dim o as Object = new TestTypeMembers101()
		   o.fun = 10	
		   o = new TestTypeMembers10()            		
		   o.fun = 10	
        End Sub
End Class 
