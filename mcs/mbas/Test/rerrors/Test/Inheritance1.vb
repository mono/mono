' Expected: System.InvalidCastException: Specified cast is not valid. 

Imports System
Imports Nunit.Framework

Public Class C1
End Class
Public Class C2
    Inherits C1
End Class

<TestFixture> _
Public Class Inheritance1 
                <Test, ExpectedException (GetType (System.InvalidCastException))> _
                Public Sub TestForException ()
        		Dim b As C2 = New C1()
            End Sub
	End Class
