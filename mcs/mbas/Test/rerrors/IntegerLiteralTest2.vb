'Unhandled Exception: System.OverflowException: Arithmetic operation resulted in
'an overflow.

Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class FlowOver

                <Test, ExpectedException (GetType (System.OverflowException))> _
                Public Sub TestForException ()
        	    Dim i As Integer
	            i = System.Int32.MinValue
            	    i = i - 1
            End Sub
End Class
