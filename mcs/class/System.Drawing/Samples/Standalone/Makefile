all: Samples.Common.dll binary.exe clippy.exe flatten.exe scan.exe texture.exe

Samples.Common.dll: Shapes.cs Matrices.cs
	gmcs Shapes.cs Matrices.cs -r:System.Drawing.dll -target:library -out:Samples.Common.dll

binary.exe: Samples.Common.dll binary.cs binary.Designer.cs
	gmcs binary.cs binary.Designer.cs -r:Samples.Common.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll

clippy.exe: Samples.Common.dll clippy.cs clippy.Designer.cs
	gmcs clippy.cs clippy.Designer.cs -r:Samples.Common.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll

flatten.exe: Samples.Common.dll flatten.cs flatten.Designer.cs
	gmcs flatten.cs flatten.Designer.cs -r:Samples.Common.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll

scan.exe: Samples.Common.dll scan.cs scan.Designer.cs
	gmcs scan.cs scan.Designer.cs -r:Samples.Common.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll

texture.exe: texture.cs
	gmcs texture.cs -r:System.Drawing.dll -r:System.Windows.Forms.dll

clean:
	rm -f *.exe Samples.Common.dll
