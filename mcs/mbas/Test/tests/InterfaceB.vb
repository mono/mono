Interface ILeft
    Sub F()
End Interface

Interface IRight
    Sub F()
End Interface

Interface ILeftRight
    Inherits ILeft, IRight
End Interface

Class LeftRight
    Implements ILeftRight

    Sub LeftF() Implements ILeft.F
    End Sub

    Sub RightF() Implements IRight.F
    End Sub
End Class

Module InterfaceB
    Sub main()
        Dim lr As New LeftRight()
        lr.LeftF()
        lr.RightF()
    End Sub
End Module
