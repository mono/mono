' ErrorMessage: System.InvalidCastException: Cast from string "Hello " to type 'Double' 
' is not valid.


Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class InvcastEx

                <Test, ExpectedException (GetType (System.InvalidCastException))> _
                Public Sub TestInvCastEx ()
		        Dim i As Integer = 0
		        Dim str As String = "Hello "
		        str += i

               End Sub
End Class
