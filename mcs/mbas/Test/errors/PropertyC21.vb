'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
REM LineNo: 17
REM ExpectedError: BC31063
REM ErrorMessage: 'Set' method cannot have more than one parameter

Imports System
Module M
	Dim a As Integer() = {1,2,3}
	Property Prop (ByVal i As Integer) As Integer
		Get
			return a(i)
		End Get
		Set (x As Integer, ByVal Value As Integer)
		End Set
	End Property
	Sub Main ()
	End Sub
End Module
