'Expected: Syetem.DivideByZeroException, A =A/Zero is not valid '
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class ArithmeticOperators1
                                                                                
                <Test, ExpectedException (GetType (DivideByZeroException))> _
                Public Sub TestDivideByZero ()
                        Dim A As Integer
                        A = 34
                        Dim Zero As Decimal = 0
                        A = A/Zero
                End Sub
End Class











