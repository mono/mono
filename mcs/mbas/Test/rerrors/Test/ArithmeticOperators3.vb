'Expected:System.OverflowException '
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class ArithmeticOperators3
                                                                                
                <Test, ExpectedException (GetType (System.OverflowException))> _ 
               Public Sub TestOverFlow ()
                        Dim A As Decimal
                        A = System.Decimal.MaxValue
                        A = A ^ 2
                End Sub
End Class








