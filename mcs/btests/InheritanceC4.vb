REM LineNo: 9
REM ExpectedError: BC30280
REM ErrorMessage: Enum 'e' must contain at least one member.

REM LineNo: 13
REM ExpectedError: BC30258
REM ErrorMessage: Classes can inherit only from other classes.

public enum e
End enum

Public Class C2
	inherits e
End Class

Module InheritanceC3
    Sub Main()
    End Sub
End Module
