Interface I
    Function F()
End Interface

Class C
    Implements I

    Function F() Implements I.F
    End Function
End Class

Module InterfaceA
    Sub Main()
        Dim x As C = New C()
        x.F()

        Dim y As I = New C()
        y.F()
    End Sub
End Module
