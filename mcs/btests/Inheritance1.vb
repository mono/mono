' this could be a compile time exception too
' But MS vb 7.0 is throwing type cast exception at runtime
' hence I am keeping it as a Negetive-Runtime-Test candidate

Public Class C1
End Class

Public Class C2
    Inherits C1
End Class

Module InheritanceC3
    Sub Main()
        Dim b As C2 = New C1()
    End Sub
End Module
