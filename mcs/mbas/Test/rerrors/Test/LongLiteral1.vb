'Expected Exception:: InvalidCastException
Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class LongLiteral1

                <Test, ExpectedException (GetType (System.InvalidCastException))> _
                Public Sub TestForException ()
			Dim a As Long
			a="Hello"
                End Sub
End Class
