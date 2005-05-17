'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework



<TestFixture>_
Public Class LateBinding16
    Public i as integer	
Class Base123
End Class

Class Derived123
    Inherits Base123
End Class
	_<Test, ExpectedException (GetType (System.Reflection.AmbiguousMatchException))>

	Sub F(ByVal b As Base123,ByVal c As derived123)
       		i = 10
	End Sub

	Sub F(ByVal d As Derived123, ByVal c1 As Base123)
        	i = 20
	End Sub
    	Public Sub TestForException()
	        Dim b As Base123 = New Derived123()
      	  	Dim o As Object = b
	     	F(b,o)
		F(o,o) 'This will cause an exceptio to be thrown
     	End Sub
End Class 
