'System.InvalidCastException:
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class InvCast
                                                                                
                <Test, ExpectedException (GetType (System.InvalidCastException))> _
                Public Sub TestForException ()
			Dim a As Double
			a="Hello"
                End Sub
End Class
