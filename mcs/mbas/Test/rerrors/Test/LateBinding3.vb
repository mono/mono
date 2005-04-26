'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

Class Test12
	 Sub fun(ParamArray a() as Long)	
		a(1) = 10
	 End Sub
End Class

<TestFixture>_
Public Class TypeMembers3
	_<Test, ExpectedException (GetType (System.IndexOutOfRangeException ))>
        Public Sub TestForException()
		   dim i as Long  
		   dim o as Object = new Test12()
		   o.fun(i) 
        End Sub
End Class 

