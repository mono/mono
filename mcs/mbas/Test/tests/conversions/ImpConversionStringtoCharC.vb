'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionStringtoCharD
	Sub Main()
			Dim a as String = "hello"
			Dim b as Char = a + "a"
			if b <> "h"
				Throw new System.Exception("Concat of String & Char not working. Expected  'h' but got " &b) 
			End if		
	End Sub
End Module

