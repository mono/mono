REM LineNo: 10
REM ExpectedError: BC30258
REM ErrorMessage: Classes can inherit only from other classes.

public enum e
	Zero
End enum

Public Class C2
	Inherits e
End Class

Module InheritanceC3
    Sub Main()
    End Sub
End Module
