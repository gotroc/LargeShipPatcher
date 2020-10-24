## LargeShipPatcher

### A Space Haven mod

https://github.com/gotroc/LargeShipPatcher

#### Features

- Allow ships of size 3x2, 2x3 and 3x3 to be build
- Increase the available ship points from 8 to 14
- Increase sector size from 8x8 to 10x10
- Allow changing the amount of system points per ship point
- Features can be tweaked by editing the `LargeShipPatcher.xml` file
- OSX/Linux support (**untested**)

#### Instructions
- Copy the patcher in the same folder as `spacehaven.jar`
- Execute it as administrator (sudo on linux)
- Follow the instructions

#### Settings

- `enableLargeShips`
  - boolean (true / false), stock default : false, mod default : true
  - Enable or disable the extra large ship templates
- `shipPoints`
  - Integer, stock default : 8, mod default : 14
  - Amount of ship points available
- `systemPointsPerShipPoint`
  - Integer, stock default : 4, mod default : 4
  - Amount of system points per ship point
  - Increasing this isn't recommended as it gives an unfair advantage vs AI ships
- `sectorSize` 
  - Integer, stock default : 8, mod default : 10
  - min value : 8
  - **Don't use uneven values** (9, 11, 13...)
  - Size of the sector window will become too large for a 1080p display if you use a value above 12

#### Remarks and known issues

###### GOG version

The patcher will only recognize automatically the Steam version of the game. 
It will probably work with the GOG version, but you will have to select the adequate patch manually.

#### License
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
