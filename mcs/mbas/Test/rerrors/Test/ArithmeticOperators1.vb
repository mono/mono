'Syetem.DivideByZeroException: A =A/0 is not valid '
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class DivideByZero
                                                                                
                <Test, ExpectedException (GetType (DivideByZeroException))> _
                Public Sub TestDivideByZero ()
                        Dim A As Integer
                        A = 34
                        Dim Zero As Decimal = 0
                        A = A/0
                End Sub
End Class











