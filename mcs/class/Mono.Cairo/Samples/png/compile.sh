#!/bin/bash
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp arc.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp arcneg.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp clip.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp clip_img.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp curve_rect.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp curve_to.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp fillstroke.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp gradient.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp image.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp image_pattern.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp knockout.cs 
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp text.cs 
