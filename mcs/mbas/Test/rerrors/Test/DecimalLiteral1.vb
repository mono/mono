'Expected Exception: OverflowException
Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class DecimalLiteral1

                <Test, ExpectedException (GetType (System.OverflowException))> _
                Public Sub TestOverFlow ()

			Dim a1 As Decimal
			a1="Hello"

                End Sub
End Class


