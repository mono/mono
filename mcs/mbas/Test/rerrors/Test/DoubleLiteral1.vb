'System.InvalidCastException:
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class DoubleLiteral1
                                                                                
                <Test, ExpectedException (GetType (System.InvalidCastException))> _
                Public Sub TestForException ()
			Dim a As Double
			a="Hello"
                End Sub
End Class
