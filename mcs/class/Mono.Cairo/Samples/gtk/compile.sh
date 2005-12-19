#!/bin/bash
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp circles.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp arc.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp arcneg.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp clip.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp clip_img.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp curve_rect.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp curve_to.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp fillstroke.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp gradient.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp image.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp image_pattern.cs sysdraw.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp text.cs sysdraw.cs
