'Unhandled Exception: System.InvalidCastException: Cast from string "Hello" to ty
'pe 'Boolean' is not valid. ---> System.FormatException: Input string was not in
'a correct format.

Option Explicit
Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class InvaCast

                <Test, ExpectedException (GetType (System.InvalidCastException))> _ 
                Public Sub TestForException ()
	        Do While "Hello"
        	Console.WriteLine("Hello World")
        	Loop
                End Sub
End Class
