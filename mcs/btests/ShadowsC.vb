Interface ILeft
    Sub F()
End Interface

Interface IRight
    Sub F()
End Interface

Interface ILeftRight
    Inherits ILeft, IRight
    Shadows Sub F()
End Interface


Module ShadowsC
    Sub Main()
    End Sub
End Module
