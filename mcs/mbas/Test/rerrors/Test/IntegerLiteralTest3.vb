'Expected Exception:: OverflowException
Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class IntegerLiteralTest3

                <Test, ExpectedException (GetType (System.OverflowException))> _
                Public Sub TestForException ()
             	Dim l As Long
             	l = System.Int64.MaxValue
            	l = l + 1
            End Sub
End Class

