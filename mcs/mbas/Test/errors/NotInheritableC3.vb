REM LineNo: 7
REM ExpectedError: BC30607
REM ErrorMessage: 'NotInheritable' classes cannot have members declared 'MustOverride'.

Imports System
NotInheritable Class A
         Public MustOverride Sub F()
End Class

Module NotInheritableTest
	Sub Main()
	End Sub
End Module
