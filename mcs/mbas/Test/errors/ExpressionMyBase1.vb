'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
REM LineNo: 7
REM ExpectedError: BC31411
REM ErrorMessage: A must be declared 'MustInherit' because it contains methods declared 'MustOverride'

Class A
	Public MustOverride Sub F()
End Class
Class B
	Inherits A
	Public Overrides Sub F()
		MyBase.F() ' Error, MyBase.F is MustOverride.
	End Sub
End Class
