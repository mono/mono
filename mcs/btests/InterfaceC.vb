
Interface I
    Function F()
End Interface

MustInherit Class C1
    Implements I

    Function F() Implements I.F
    End Function
End Class

MustInherit Class C2
    Implements I

    MustOverride Function F() Implements I.F
End Class


Class DC1
    Inherits C1
End Class

Class DC2
    Inherits C2

    Overrides Function F()
    End Function
End Class

Module InterfaceC
    Sub Main()
        Dim x As DC1 = New DC1()
        x.F()

        Dim y As DC2 = New DC2()
        y.F()
    End Sub
End Module

