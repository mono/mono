' System.Exception

'Option Strict On
Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class LikeOperator1

                <Test, ExpectedException (GetType (System.Exception))> _
                Public Sub TestForException ()
	        Dim a As Boolean
	        a = "HELLO" Like "H[A- Z][!M-P][!A-K]O"
	        If a <> True Then
	        Console.WriteLine("#A1-LikeOperator:Unexpected behaviour")
        End If
        End Sub
End Class
