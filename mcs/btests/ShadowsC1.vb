Class B
    Function F()
    End Function

    Function F(ByVal i As Integer)
    End Function


    Function F(ByVal i As String)
    End Function
End Class

Class D
    Inherits B
    ' all other overloaded methods should become unavailable 
    Shadows Function F()
    End Function
End Class

Module ShadowA_C1
    Sub Main()
        Dim x As D = New D()
        x.F(10)
        x.F("abc")
    End Sub

End Module

