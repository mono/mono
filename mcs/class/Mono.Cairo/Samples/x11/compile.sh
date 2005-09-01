#!/bin/bash
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp arc.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp arcneg.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp clip.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp clip_img.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp curve_rect.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp curve_to.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp fillstroke.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp gradient.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp image.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp image_pattern.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp text.cs x11.cs
