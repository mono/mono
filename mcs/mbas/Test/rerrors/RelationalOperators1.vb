'Unhandled Exception: System.InvalidCastException: Cast from string "Hello" to type
'Double' is not valid. ---> System.FormatException: Input string was not in a correct format

Option Explicit
Imports System
Imports Nunit.Framework
<TestFixture> _
Public Class RelationalOper
		<Test, ExpectedException (GetType (System.InvalidCastException))> _
		 Public Sub TestForException()
        	 Dim a As Long = 0
        	 Dim b As String = "Hello"
        	 If a < b Then
            	 Console.WriteLine("#A1-RelationalOperator: Not Expected")
       		 End If
    		 End Sub
End Class
