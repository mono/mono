'Expected Exception:: InvalidCastException
Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class IntegerLiteral1

                <Test, ExpectedException (GetType (System.InvalidCastException))> _
                Public Sub TestForException ()
			Dim a As Integer
			a="Hello"
                End Sub
End Class
