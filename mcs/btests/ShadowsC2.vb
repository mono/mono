Class B
    Function F()
    End Function

    Function F(ByVal i As Integer)
    End Function
End Class

Class D
    Inherits B

    Overloads Shadows Function F()
    End Function
End Class

Module ShadowsC2
    Sub Main()
    End Sub
End Module
