'Unhandled Exception: System.InvalidCastException: Cast from string "Hello" to type
'Double' is not valid. ---> System.FormatException: Input string was not in a
' correct format.


Imports System
Imports Nunit.Framework
<TestFixture> _
Public Class StringLiterals1
                <Test, ExpectedException (GetType (System.InvalidCastException))> __
		Public Sub TestForException ()
			Dim a As String= "Hello"
			Dim b As String= "World"
			Dim c As String= a*b
	End Sub
End Class
