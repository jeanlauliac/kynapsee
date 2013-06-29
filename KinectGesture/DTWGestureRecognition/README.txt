---------------------------------------------------------------------
KinectDTW - Kinect SDK Dynamic Time Warping (DTW) Gesture Recognition
---------------------------------------------------------------------
By Chin Xiang Chong
Based off code originally provided by Rhemyst and Rymix from http://kinectdtw.codeplex.com
Large parts of this readme file are taken from their original readme file as well!
www.waxinlyrical.com/codesamples/KinectGestures


Notices
-------
This source code is distributed freely as open source. 

Microsoft's SDK is not for commercial use, so by extension neither is this 
library. Always adhere to Microsoft's terms of Kinect SDK use: 
http://research.microsoft.com/en-us/um/legal/kinectsdk-tou_noncommercial.htm

No warranty or support given. No guarantees this will work or meet your needs. 


Introduction
------------
This is a demo for gesture recognition for the Kinect using a Dynamic Time Warping algorithm. 
For those unfamiliar with that term, 
http://web.science.mq.edu.au/~cassidy/comp449/html/ch11s02.html
provides a good primer. 
The Dynamic Time Warping element means that this can recognise gestures performed at different speeds.


Instructions
------------
1. Open up the Solution and check that you have all the prerequisite 
software/dlls. Put Visual Studio into Debug mode and build and run the project.
You should see the MainWindow XAML window appear. If anything goes wrong at 
this point, check your references, any paths that might be wrong or any other 
dependencies that I might have forgotten.

2. Step into view of the Kinect sensor. Your skeleton should be tracked almost 
immediately. It's probably best is you do this with only you in Kinect's sight 
as this release only deals with one player.

3. Load the sample gestures by clicking Load gesture file and navigating to 
the supplied DefaultGestures.txt file

4. Start performing some gestures. You can see the names of the gestures from 
the select box, and hopefully most of them are obvious to perform. You will 
see matches appear in the results text panel at the top of the screen.

5. Nuke the app and start again. 

6. Try recording your own gestures. Make sure your skeleton is being tracked, 
select the gesture name you want to record, then click the Capture button. 
You have three seconds to get into place and start recording your gesture. The 
gesture is currently hard-coded to look at 32 frames (which is actually every 
other frame over 64 expended frames). You may want to tweak this setting.

   When recording gestures it is important that you start your gesture as soon 
   as the recording starts and that you finish on the 32nd frame. This might 
   mean that you have to perform your gestures for the recording slower (or 
   perhaps more quickly) than you would do in real life. Stick with it. The 
   DTW algorithm doesn't care about how quickly the gesture is performed.

7. When recording of each gesture is finished it automatically switches back 
into Read mode, so test your new gesture a few times to see if you're happy 
with it. If not, re-record it and try again.

8. When you're happy with your results, save your gestures to file. 

9. Make your own gestures - simply amend or add to the selectbox items with a 
unique name and record your gesture. Note that a gesture name must start with 
an @

That's it for this demo. Of course, it's not production specification yet, nor 
would you want to release the gesture recorder in your project (only the 
recogniser), but hopefully this gives you a good start in producing your own 
gestures. DTW is probably not the perfect solution for all gestures, either, so
you'll need to experiment to find out what works and what doesn't. However, it 
is a powerful tool for general gesture recognition.


Features
--------
* 3D gesture recognition using all the joints of the upper torso (Skeleton 
Frame)

* Fast and customisible gesture recogniser

* Gesture recorder, so you can create your own gestures

* Save gestures to file for future use (and load from file)

* Sample WPF project with skeletal viewer and optional depth and RGB viewers


Requirements
------------
Windows 7
Kinect SDK
Visual Studio 2010
XNA Framework
Probably the latest .NET and maybe some other stuff too. You'll soon find out. 
Look at the project References if you're stuck.


Tested on
---------
Windows 7 Professional 64-bit SP1
Visual Studio 2010 Ultimate
Intel i5-750
8.00 GB RAM 
NVidia 470


Links
-----
My personal page:
	www.waxinlyrical.com/codesamples

KinectDTW project on Codeplex:
	http://kinectdtw.codeplex.com/

Microsot's Kinect SDK terms of use:
	http://research.microsoft.com/en-us/um/legal/kinectsdk-tou_noncommercial.htm

Wikipedia's ovbiously 100% reliable explanation of dynamic time warping:
	http://en.wikipedia.org/wiki/Dynamic_time_warping
	
Good alternative write up on DTW:
	http://web.science.mq.edu.au/~cassidy/comp449/html/ch11s02.html


Disclaimer
----------
This is a student project, please don't sue me if anything goes wrong :/ 