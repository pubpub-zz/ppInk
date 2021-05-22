# Changes of this fork (https://github.com/jorgecc/ppInk-1)

* 2021-05-22
* It adds a feature to clean the canvas every time we hide our pencil. By default, this
feature is off.
* In the options windows, it adds a new tab to configure this value.
* Root.cs lots of cleanups (Microsoft suggesting of style of code)


 #### Introduction

ppInk is an on-screen annotation software under Windows, forked from gInk.

![](screenshot1.png)

ppInk introduces many new features compared to gInk, greatly inspired by another
screen annotation software Epic Pen, but even more easy to use. ppInk / gInk are
made with the idea kept in mind that the interface should be simple and should
not distract attention of both the presenter and the audience when used for
presentations. Unlike in many other softwares in the same category, you select
from pens to draw things instead of changing individual settings of color,
transparency and tip width everytime. Each pen is a combination of these
attributes and is configurable to your need.

I want his tool to be usable through mouse but also throug touchscreen or graphic tablet. 

A set of drawing tools are introduced: Hand Writing, Line (2 points and polyline), Rectangular,
Ellipsis, Arrow, Numbering Tag, Text Left/Right aligned, cliparts

![](screenshot2.png)

In order to reduce the number of buttons, some buttons have multiple functions,
selected my multiple click on those or some through long or right click:

-   Hand / Rectangular / Ellipsis :  
    unfilled drawing -\> filled with pen color -\> filled with white -\> filled
    with black
    
-   Line:
    2 points -\> polyline -\> pen color filled polygon -\> white filled polygon -\> black filled polygon

-   Numbering :  
    transparent background -\> edit tag Number -\> white background -\> black background

-   Arrow :
    Arrow draw at the beginning -\> Arrow draw at the end
    
    (*) : head position when tool is first selected can be modified by option

-   Text:  
    Text left aligned -\> Text Right aligned
    You can now use ctrl+enter to validate the entry

-   Move:  
    Move 1 drawing -\> Copy 1 drawing -\> Move all drawings.
    
-   Edit:
    Short click : modify text/number tool (text and font) *or* the color/pen width of a stroke
    Long click : edit default font (for the time of the session)

-   Cliparts:
    Open the dialog to select image and the background/frame. This list displayed of cliparts is initialized from the list defined 
    in the options. you can overload adding images for the inking sessions from files or from the clipboard.
    you can then stamp the images. you just click, the image will be stamped with the images original size. 
    if you just draw a vertical or horizontal line, the image will be drawn with the drawn width/height respecting 
    proportional size. else you can draw a box which will define the image size (non proportional). 
    If you draw from bottom/right to up/left, the image will be stamped reversed.
    
    3 preset cliparts are also available: they can be configured in the option dialogbox, or for the time of the session through a right or long
    click on the selected button.

*Magnetic*
The magnet activates some magnetic effect :
-   Find a point on the drawing next to the cursor. For rectangles, magnetic
    effect is available to vertices, and also to the middle of sides.(also activated pressing Ctrl)
-   The 8 points of the bounding rectangle of a text.(also activated pressing Ctrl)
-   On the line from the origin. The lines are horizontal,vertical and every 15�.(also activated pressing Shift)
The Magnetic distance can be adjusted in the options.
If only Ctrl or Shift is depressed, the global magnetic effect is switched off to only take into account the magnetic of the pressed key.
Hotkeys are availables for all tools,pens (configurable throught right click on icon next to clock)

*Move one -> Move All(pan) -> Copy one*
 You can move one stroke when clicking first time on the button. The stroke to be moved/copied will be surrounded by a rectangle to identify it.

*Zoom*
Two zoom can be activated (multiple clicks on the zoom buttom). the first one offers a standard dynamic window following the cursor. with the second one, you select the area that you want to enlarge. this area will be frozen and then full displayed on the screen. You can then carry on drawing on this new enlarged background image. a new click will end working on this image and will show back the screen and will restore the previously drawn strokes.

*Save / Load*
through those two buttons, you will be able to store(in a text format) the current strokes. Load redraw the saved strokes onto the existing drawing.
- Save button  : a long click(or first short click) is a sort of "save as" : it open the dialog box and will allow you to select the filename. the following short clicks will overwrite the strokes into the previously named file.
note that a backup is done when inking is ended/quit.

Load button : a long click(or first short click) is a sort of "load as" : it open the dialog box and will allow you to select the filename. the following short clicks will load the strokes from the previously named file. at first click the file loaded is the autosave(from latest session).

*Alt+ shortcut for temporary commands:*
When this option is activated (yes by default), when Alt is pressed and hold, the tool/pen/mode is temporary selected, left when alt is released
eg : with Hand drawing selected, press Alt and keep it  down. Press and release R : rectangle is active, as long as Alt is depressed, and Hand will be reengaged when Alt is released
This can be reset any combinaisons of Pens and Tools : eg : press Alt, you can engage Filled Blue rectangle, depressing R twice, and 3 (in any order), and return to previous tools/color releasing Alt
This can be also used with Erasor
when pressing down Alt, the cursor is also temporary change to the big arrow to ease finding it on screen.

*Option Capture at toolbar opening*
Capture can be engaged as toolbar is opened. This option is set to false by default.

*Long left click/Right click on Pens*
Open the Modify pen dialog box for that pen

*Clear Board (Bin icon):*

short click : Delete all drawings and apply last selected background
Long click : Delete all drawings and select background surface color (white/black/customed color(modifiable through Options))
In options you will be able to select a default mode at opening , and customed color
advice : the created rectangle can be deleted using the erasor next to the border of the screen.

*Fading color*
When defining a color, you can check Fading : in this case the stroke drawn will dissapear after the defined time(adjustable in options dialogbox/pen tab)

*cursor files:*
You can configure you own cursor file, saving it as cursor.ico in your exe folder (click position and size are loaded from the file). In the same you can save an image as FloatingCall.png to define the calling form image(in this case the width and transparency are in the Window_POS parameter in config.ini
*mouse wheel:*
Mouse wheel allows you to modify the pen width. this can be easily observed with the tipped cursor. in this configuration, finding the mouse may be difficult to find in this configuration : you can then depress the alt key to get the arrow(or customized) cursor
shift+mouse wheel allows to select pen
mouse wheel / shift+ mouse wheel can now be swapped (shift+mouse wheel to access pen width) : available in options / pen tab

*video recording:*
ppInk has now some capability to do some video recording. tuning is available in options/video tab :
   - basic recording with FFmpeg:
   You need to first install ffmpeg. and select the option in the video tab. In this case the button will be proposed. you can start/stop recording. Names and destination folders are defined throught the command line.
   - advanced recording with OBS-studio:
   You need to first install and configure OBS-studio(https://obsproject.com/fr/download) with OBS-websocket(https://github.com/Palakis/obs-websocket). select the required option (recording or broadcasting). with this program you will be able to start/stop broadcasting or start/pause/resume/stop video recording. file names and folders are defined in OBS-studio.
   Note : ppink is now compatible with release 4.8 of OBS-websocket. this should be fixed in very next release. for the moment,prefer to stop recording when stopping Ink mode

*UI customisation*
You add arrow.ani/cur/ico and eraser.ani/cur/ico to put your own cursors. if you use ico file the cursor is down with the file sized ( you can then put a bigger or smaller image)
toolbar Background Color can be changed in the options. currently a draw hickup make the background color incorrect during opening. transparency is currenly ignored
Button Images can be customized putting Png files in the ppink exe folder. the name to be used are the same as the one from the src folder.

*Toolbar orientation*
You can now select how the toolbar will be deployed : to left/ to right or vertically to top/ to bottom

*Alt+Tab engaging Pointer*
When the option is set in the options, switching application (with alt+tab) will engage pointer mode. Also, when pointer mode is activated (by any means, ie button click, alt+tab, global short cut), the toolbar is folded automatically, and when pressing undock button, alt+tab, or global shortcut,the inking mode is restored and the toolbar is unfolded.
Note that you can still fold toolbar when drawing without engaging pointer mode with the dock button.

*Window mode*
You can now run ppink in window mode (engaged through Long/Right Click on pointer icon)
In this mode ppInk is run in a window and you can access clicks or mousewheel applications out of the window:
https://user-images.githubusercontent.com/4083478/112311221-c656c580-8ca5-11eb-895b-2279366c0fc4.mp4>

*snapshots in pointer mode*
when trying to annote menu opened with mouse click / contextual menus
https://user-images.githubusercontent.com/61389799/111090958-1d3bfc80-853a-11eb-91fc-04e85ed18454.mp4
(demo from @eamayreh)

you can configure shortcuts with shift/ctrl/alt with a press and hold and tap twice keys to prevent menu to close
multiple snapshots can be captured, they are pasted one over the other, in the reverse order to make a full sequence


#### Download

<https://github.com/PubPub-zz/ppInk/releases/>

#### Change log

<https://github.com/pubpub-zz/ppInk/blob/master/changelog.txt>


#### How to use

Start ppInk.exe and an icon will appear in the system tray and possible a floating window(\*)
(which can be moved using RightClick) to start drawing on screen.  
Inking is started :
 - clicking on floating icon
 - clicking on the icon in the system tray
 - using the global shortcut (ctr+alt+G by default)
 - immediately after start Pping if "--startInking" (case insensitive) has been added tocommand line
 - ppInk is run once more (no extra instance is started)

(\*) activation and position saving are available in options.

Click the exit button or press ESC to exit drawing.

#### Features

-   Compact and intuitive interface with customizable hotkeys.

-   Inks rendered on dynamic desktops.

-   Drawing tools: Hand Writing, Line, Rectangular, Ellipsis, Arrow, Numbering
    Tag, Text Left/Right aligned

-   Stylus with eraser, touch screen and mouse compatible.

-   Click-through mode. (note : once inking is engaged, global shortcut enters and exits this mode)

-   Multiple displays support.

-   Pen pressure support.

-   Snapshot support.

-   Hotkey support.

-   Magnetic effect when drawing shapes

-   Filled shapes

-   Video recording

-   Load/Save stroke

-   Zoom (2 versions)

#### Tips

-   There is a known issue for multiple displays of unmatched DPI settings
    (100%, 125%, 150%, etc.). If you use gInk on a computer with multiple
    displays of unmatched DPI settings, or you encounter problems such as
    incorrect snapshot position, unable to drag toolbar to locations etc.,
    please do the following as a workaround (in Windows 10 version 1903 as an
    example): right-click ppInk.exe, Properties, Compatibility, Change high DPI
    settings, Enable override high DPI scaling behavior scaling performed by:
    Application. (only after v1.0.9, which will be released soon)

-   There is a very few hidden options you can tweak in config.ini that are not
    shown in the options window.

#### How to contribute translation

gInk/ppInk supports multiple languages now (ppInk introduces a few new sentences
where internationalization has not be implemented.). Here is how you can
contribute translation. Simply create a duplication of the file "en-us.txt" in
"bin/lang" folder, rename it and then translate the strings in the file. Check
in ppInk to make sure your translation shows correctly, and then you can make a
pull request to merge your translation to the next version of release for others
to use.

NEW :  arabic available

gInk  
https://github.com/geovens/gInk  
https://github.com/geovens/gInk  
Weizhi Nai \@ 2019

ppInk
https://github.com/pubpub-zz/ppInk
Pubpub-ZZ \@ 2020






