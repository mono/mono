'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionBytetoStringB
	Sub Main()
			Dim a as Byte = 255
			Dim b as String = "111" + a
			if b <> "366"
				Throw new System.Exception("Concat of Byte & String not working. Expected 366 but got " &b) 
			End if		
	End Sub
End Module

