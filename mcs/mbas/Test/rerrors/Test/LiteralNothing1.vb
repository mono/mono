' Unhandled Exception: System.NullReferenceException: Object reference not set to
'an instance of an object.

Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class NullRef

                <Test, ExpectedException (GetType (System.NullReferenceException))> _
                Public Sub TestForException ()
			Dim a As String="Hello"
			a=Nothing
			Dim b As String=a.Substring(2)
                End Sub
End Class
