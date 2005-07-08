'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
REM LineNo: 17
REM ExpectedError: BC30237
REM ErrorMessage: Parameter already declared with name 'i'

Imports System
Module M
	Dim a As Integer() = {1,2,3}
	Property Prop (ByVal i As Integer) As Integer
		Get
			return a(i)
		End Get
		Set (ByVal i As Integer)
			a (0) = i
		End Set
	End Property
	Sub Main ()
	End Sub
End Module
