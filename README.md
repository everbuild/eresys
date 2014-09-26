## Eresys


Half-Life 1 map renderer using C#/.NET/DirectX. It was an interesting experience 
in loading and rendering BSP maps. Also contains an attempt at collision detection.

This piece of software is the result of a college project I worked on together with some peers
somewhere back in 2003-2004. Mostly out of nostalgia, I decided to try to revive it and donate 
it to the public domain. I think/hope my former collegues won't mind.

The goal was to develop a 3D engine that could render Half-Life 1 maps. I totally love that game,
so the choice is obvious. C# and DirectX were chosen because we learned C# that year and I already 
had some familiarity with DirectX. Another team would craft a map of the school campus to go with it. 
Unfortunately, the map didn't quite make it that far...

The slightly peculiar name stands for **E**xperimental **Re**ndering **Sys**tem.

Almost 10 years after it's conception, it turned out to be
pretty hard to get the code to build and run again. This mostly because of the
evolution of the .NET framework and managed DirectX no longer being supported.
It really deserves a port to a more modern platform.
If anyone wants to give it a try, good luck and let me know.


### Prerequisits

* DirectX: any version as of 9.0c should work. [9.0c June 2010 release](http://www.microsoft.com/en-us/download/details.aspx?id=8109)
* [.NET Framework 1.1](http://www.microsoft.com/en-us/download/details.aspx?id=26).
  Later versions are NOT backwards compatible so you really need 1.1,
  which can unfortunately be a bit tricky on more recent Windows systems.
* You'll need some HL1 BSP map (Counter-strike maps are very useful for example) and 
  associated WAD file(s) to get it to run. I don't have the rights to include any, sorry.


###	Controls

**Escape** = exit application

**Arrows** = Move forward/backward/left/right

**Right shift** = Move up

**Right control** = Move down

**P** = Reset position

**Mouse scroll** = zoom

**Print Screen** = Take screenshot - saved next to the executable as [001..999].jpg

**C** = Toggle collision detection

**Z** = Toggle wireframe rendering

**F** = Toggle frustum culling

**B** = Toggle BSP culling

**F**2 = Reset brightness, contrast and gamma correction

**F3/F4** = Decrease/increase brightness

**F5/F6** = Decrease/increase contrast

**F7/F8** = Decrease/increase gamma correction


###	Configuration

File: `eresys.ini`

```
[settings]
graphics=directx (only supports directx for now)
fullscreen=true|false
width=<window width or horizontal screen resolution, eg: 1024>
height=<window height or vertical screen resolution, eg: 768>
depth=16|24|32 (color depth in bits/pixel)
hal=true|false (use hardware acceleration or software emulation)
tnl=true|false (hardware accelerated transform & lighting)
vsync=true|false
brightness=0.5
contrast=0.5
gamma=1
controls=directx (only supports directx for now)
profiler=true|false
map=<filename>.bsp
```

All required files for maps (BSP, WAD, BMP for skymap) should reside in the same
folder as the executable (e.g. bin folder).


###	Command-line arguments

```
Eresys.exe
	-ini=<config file>.ini (overrides default name eresys.ini)
	-map=<filename>.bsp (overrides map from config file)
```

	
###	Log files

The engine generates (and overwrites) 2 log files on each run:
	
* `eresys.log` main application log, contains usefull info on errors or warnings
* `entityscript.log` the entity script from the last loaded BSP file