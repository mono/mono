'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module M
      Sub Main()		
		Static x as date
		if x <> "1/1/0001"
			Throw new System.Exception("Static declaration not implemented properly. Expected 1/1/0001 but got " &x)
		End if
      End Sub
End Module
