Class B
    Public Overridable Sub F1()
    End Sub

    Public Overridable Sub F2()
    End Sub
End Class

Class D
    Inherits B

    Public NotOverridable Overrides Sub F1()
    End Sub

    Public Overrides Sub F2()
    End Sub
End Class

Class D1
    Inherits D

    Public Overrides Sub F2()
    End Sub
End Class

Module OverrideB
    Sub Main()
    End Sub
End Module
