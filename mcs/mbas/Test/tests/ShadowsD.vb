Class B
    Public Shared Sub F()
    End Sub
End Class

Class D
    Inherits B

    Private Shared Shadows Sub F()
    End Sub
End Class

Class D1
    Inherits D
End Class

Module ShadowsD
    Sub Main()
        Dim x As New D1()
        x.F()
    End Sub
End Module
