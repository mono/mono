'Expected Exception: OverflowException
Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class Flowover

                <Test, ExpectedException (GetType (System.OverflowException))> _
                Public Sub TestOverFlow ()
		Try
			Dim a As Decimal
			a="Hello"
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
                End Sub
End Class


