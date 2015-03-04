
CSC = mcs
CAIRO = -r:Mono.Cairo.dll
GTK = -pkg:gtk-sharp-2.0
REFS = $(GTK) $(CAIRO)

all: arc.exe arcneg.exe clip.exe clip_img.exe curve_rect.exe curve_to.exe \
	fillstroke.exe gradient.exe image.exe image_pattern.exe text.exe

arc.exe: arc.cs
	$(CSC) $(REFS) arc.cs sysdraw.cs

arcneg.exe: arcneg.cs
	$(CSC) $(REFS) arcneg.cs sysdraw.cs

clip.exe: clip.cs
	$(CSC) $(REFS) clip.cs sysdraw.cs

clip_img.exe: clip_img.cs
	$(CSC) $(REFS) clip_img.cs sysdraw.cs

curve_rect.exe: curve_rect.cs
	$(CSC) $(REFS) curve_rect.cs sysdraw.cs

curve_to.exe: curve_to.cs
	$(CSC) $(REFS) curve_to.cs sysdraw.cs

fillstroke.exe: fillstroke.cs
	$(CSC) $(REFS) fillstroke.cs sysdraw.cs

gradient.exe: gradient.cs
	$(CSC) $(REFS) gradient.cs sysdraw.cs

image.exe: image.cs
	$(CSC) $(REFS) image.cs sysdraw.cs

image_pattern.exe: image_pattern.cs
	$(CSC) $(REFS) image_pattern.cs sysdraw.cs

text.exe: text.cs
	$(CSC) $(REFS) text.cs sysdraw.cs

