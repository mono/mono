'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module NewTest
	Structure Point
		Public x,y,A As Integer
		Public Sub New(ByVal x As Integer, ByVal y As Integer)
			Me.x= x 
			Me.y = y
		End Sub
      
		Public Sub Add
			A = Me.y +Me.x
			if A <> 701 Then 
				Throw New Exception ("Unexpected behavior:: A should be equal to Me.X + Me.Y=300+400 = 700 but got A=" &A)
			End If
		nd Sub
	End Structure
	
	Sub Main()
		im R as New Point (300,400)
	      R.Add
	End Sub	
End Module
