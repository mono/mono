
CSC = mcs
CAIRO = -r:Mono.Cairo.dll

all: arc.exe arcneg.exe clip.exe clip_img.exe curve_rect.exe curve_to.exe \
	fillstroke.exe gradient.exe image.exe image_pattern.exe text.exe

arc.exe: arc.cs
	$(CSC) $(CAIRO) arc.cs x11.cs

arcneg.exe: arcneg.cs
	$(CSC) $(CAIRO) arcneg.cs x11.cs

clip.exe: clip.cs
	$(CSC) $(CAIRO) clip.cs x11.cs

clip_img.exe: clip_img.cs
	$(CSC) $(CAIRO) clip_img.cs x11.cs

curve_rect.exe: curve_rect.cs
	$(CSC) $(CAIRO) curve_rect.cs x11.cs

curve_to.exe: curve_to.cs
	$(CSC) $(CAIRO) curve_to.cs x11.cs

fillstroke.exe: fillstroke.cs
	$(CSC) $(CAIRO) fillstroke.cs x11.cs

gradient.exe: gradient.cs
	$(CSC) $(CAIRO) gradient.cs x11.cs

image.exe: image.cs
	$(CSC) $(CAIRO) image.cs x11.cs

image_pattern.exe: image_pattern.cs
	$(CSC) $(CAIRO) image_pattern.cs x11.cs

text.exe: text.cs
	$(CSC) $(CAIRO) text.cs x11.cs

