REM LineNo: 15
REM ExpectedError: BC30583
REM ErrorMessage: 'I.F1' cannot be implemented more than once.

Interface I
    Function F1(ByVal i As Integer)
End Interface

Class B
    Implements I

    Overridable Function CF1(ByVal i As Integer) Implements I.F1
    End Function

    Overridable Function CF2(ByVal i As Integer) Implements I.F1
    End Function
End Class

Module InterfaceC3
    Sub Main()
    End Sub
End Module
