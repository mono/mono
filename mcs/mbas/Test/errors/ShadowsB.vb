REM LineNo: 30
REM ExpectedError: BC40004
REM ErrorMessage: function 'F' conflicts with function 'F' in the base class 'B' and so should be declared 'Shadows'.

' As per MS VB Specification (section 4.3.3)
' this program should compile.
' But MS VB compiler 7.0 is unable to compile it.
' Still I am keeping this in positive test cases
' May move it later to negative tests section 
' after clarifying it with later versions of MS VB compilers.

' In derived class if you 
' override a method whithout
' shadowing or overloading should get shadowed 
' in the derived class by default
' But it should throw an warning during compilation


Class B
    Function F()
    End Function

    Function F(ByVal i As Integer)
    End Function
End Class

Class D
    Inherits B

    Function F()
    End Function
End Class

Module ShadowsB
    Sub Main()
    End Sub

End Module
