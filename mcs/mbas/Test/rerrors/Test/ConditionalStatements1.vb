'Expected Exception:: 
'System.InvalidCastException: Cast from string "String" to type 'Boolean' is not valid.

Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class InvCast

                <Test, ExpectedException (GetType (System.InvalidCastException))> _
                Public Sub TestForException ()
      	if "String" Then
		throw new exception("#CSC2")
	end If
                End Sub
End Class
