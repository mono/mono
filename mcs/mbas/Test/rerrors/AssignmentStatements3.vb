' System.OverflowException: Arithmetic operation resulted in an overflow.

Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class FlowOver

                <Test, ExpectedException (GetType (System.OverflowException))> _
                Public Sub TestOverFlow ()
	        	Dim b As Integer = 0
       		 	b += 1000
            	End Sub
End Class

