REM LineNo: 12
REM ExpectedError: BC31408
REM ErrorMessage: 'MustInherit' and 'NotInheritable' cannot be combined.

REM LineNo: 13
REM ExpectedError: BC30607
REM ErrorMessage: 'NotInheritable' classes cannot have members declared 'MustOverride'.

'Testing a class that is declared as both mustinherit and notinheritable

Imports System                                                                                                                             
NotInheritable MustInherit Class A
          Public MustOverride Sub F()
End Class

Module MustInheritTest
	Sub Main()
		
	End Sub
End Module

