REM LineNo: 11
REM ExpectedError: BC30389
REM ErrorMessage: 'C1.InnerC' is not accessible in this context because it is 'Protected'.

Public Class C1
	Protected Class InnerC
	End Class
End Class

Public Class C2
	inherits C1.InnerC
End Class

Module InheritanceC3
    Sub Main()
    End Sub
End Module
