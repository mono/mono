REM LineNo: 23
REM ExpectedError: BC30176
REM ErrorMessage: Only one of 'Public', 'Private', 'Protected', 'Friend', or 'Protected Friend' can be specified.

REM LineNo: 24
REM ExpectedError: BC30176
REM ErrorMessage: Only one of 'Public', 'Private', 'Protected', 'Friend', or 'Protected Friend' can be specified.

REM LineNo: 25
REM ExpectedError: BC30176
REM ErrorMessage: Only one of 'Public', 'Private', 'Protected', 'Friend', or 'Protected Friend' can be specified.

REM LineNo: 26
REM ExpectedError: BC30176
REM ErrorMessage: Only one of 'Public', 'Private', 'Protected', 'Friend', or 'Protected Friend' can be specified.

REM LineNo: 27
REM ExpectedError: BC30176
REM ErrorMessage: Only one of 'Public', 'Private', 'Protected', 'Friend', or 'Protected Friend' can be specified.

Imports System
Class C1
	Public Private a As Integer
	Public Protected b As Integer
	Public Friend c As Integer
	Private Protected d As Integer
	Private Friend e As Integer
End Class
Module Accessibility
	Sub Main()
	End Sub
End Module
