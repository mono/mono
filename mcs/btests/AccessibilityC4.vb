REM LineNo: 9
REM ExpectedError: BC31047
REM ErrorMessage: Protected types can only be declared inside of a class.

REM LineNo: 12
REM ExpectedError: BC31089
REM ErrorMessage: Types declared 'Private' must be inside another type.

Protected Class C1
End Class

Private Class C2
End Class

Module Accessibility
	Sub Main()
	End Sub
End Module

