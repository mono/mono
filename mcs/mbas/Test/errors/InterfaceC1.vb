REM LineNo: 18
REM ExpectedError: BC30375
REM ErrorMessage: 'New' cannot be used on an interface.

Interface I
    Function F()
End Interface

Class C
    Implements I

    Function F() Implements I.F
    End Function
End Class

Module InterfaceC1
    Sub Main()
        Dim x As I = New I()
    End Sub
End Module
