MustInherit Class B
    MustOverride Function F()
End Class

Class D
    Inherits B

    Shadows Function F()
    End Function
End Class

Module ShadowsC3
    Sub Main()
    End Sub
End Module
