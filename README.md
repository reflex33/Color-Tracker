# c# Color Tracker

## v1.0
AForge doing everything version.  Here, filters are in HSL color space, and AForge converts the image to HSL for each filter you want to find.

## v2.0
Re-written to use our own conversion to a new color space.  Now using HSB color space, the software only converts the image once, then each filter is applied.  Should scale better with more filters.  Tests indicate that speed is slightly better that version 1 for all test cases until we max out the number of cores of the CPU.  Then this new version is 10-20% better.

## TODO
N/A