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
