'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionStringtoIntD
	Sub Main()
			Dim a as String = "12"
			Dim b as Integer = a + 123
			if b <> 135
				Throw new System.Exception("Concat of String & Int not working. Expected  135 but got " &b) 
			End if		
	End Sub
End Module

