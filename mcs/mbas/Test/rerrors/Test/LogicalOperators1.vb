'Unhandled Exception: System.InvalidCastException: Cast from string "xyz" to type
'Boolean' is not valid. ---> System.FormatException: Input string was not in a
'correct format.

Imports System
Imports Nunit.Framework
<TestFixture> _
Public Class InvCast
                <Test, ExpectedException (GetType (System.InvalidCastException))> _ 
                Public Sub TestForException ()
	        Dim a1 As Boolean = True 
        	Dim b1 As String = "xyz"
        	If a1 And b1 Then
            	a1 = False
        	End If
                End Sub
End Class
