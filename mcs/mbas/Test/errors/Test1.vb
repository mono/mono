REM LineNo: 11
REM ExpectedError: BC30433
REM ErrorMessage: Methods in a Module cannot be declared 'MustOverride'.

Module Test

Public Sub Main()
	System.Console.WriteLine ("1st Test")
End Sub

Public MustOverride Function F()

End Module

