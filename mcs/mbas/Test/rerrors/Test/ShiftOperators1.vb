'Unhandled Exception: System.InvalidCastException: Cast from string 
' to type 'Long' is not valid. ---> System.FormatException: Input string was not in a
' correct format.
' Strict On disallows implicit conversions from double to long
'Option Strict On


Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class ShiftOperators1


                <Test, ExpectedException (GetType (System.InvalidCastException))> _
                Public Sub TestForException ()
	        Dim b1 As String = "xyz"
        	b1 = b1 << 109
    		End Sub
End Class
