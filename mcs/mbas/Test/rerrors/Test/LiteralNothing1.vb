' Unhandled Exception: System.NullReferenceException: Object reference not set to
'an instance of an object.

Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class LiteralNothing1

                <Test, ExpectedException (GetType (System.NullReferenceException))> _
                Public Sub TestForException ()
			Dim a2 As String="Hello"
			a2=Nothing
			Dim b As String=a.Substring(2)
                End Sub
End Class
