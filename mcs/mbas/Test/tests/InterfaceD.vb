Interface I
    Function F1(ByVal i As Integer)
    Function F2(ByVal i As Integer)
End Interface

Class C
    Implements I

    Function F(ByVal i As Integer) Implements I.F1, I.F2
    End Function
End Class

Module InterfaceD
    Sub Main()
        Dim x As C = New C()
        x.F(10)
    End Sub
End Module
