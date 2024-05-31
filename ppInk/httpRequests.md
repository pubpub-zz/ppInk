**General rules for response:**

**500** : an exception occurs during request processing : returns a text message
for more details

**409** : not in inking mode : incompatible with request : returns a text
message

**400** : error in parsing request (wrong parameters) : returns a text message

**404** : Command (Path part) not implemented : returns a text message

**200 :** Request successfully performed: returns a JSON response describing
current status

**General rules for queries:**

the server url root part (inhere http://localhost:7999/ may be changed in
options dialogbox

for simplicity, Parameters are passed with a single letter but are most of the
time similar to the Json respons

__Caution : The parameters tags are case sensitive__(http://localhost:7999/Inking?S=True is correct but not http://localhost:7999/Inking?s=True)

Booleans Input (True/False) are case Insensitive (and full lower in responses to
be in accordance with JSON)

**http://localhost:7999/Inking[?S=True\|False]**

*Start/Stop Inking*

S : Starting = True / False ; omitted just return status

JSON fields:

Started

**http://localhost:7999/PenDef?P=n[&Browse][&R=nnn&G=nnn&B=nnn][&T=nnn][&W=nnn.n][&F=True\|False\|nnn.n][&L=aaaaa]**

*Modify Pen*

P = number -1(currentPen),0-9 , *mandatory*

Browse : to open editor, in alternative to passing other parameters

R G B = color (0-255)

T = Transparency (0-255) opposite of Alpha

W = Width (float)

F = Fading(True,False, or float (time in sec)

L = Line Style (Stroke, Solid, Dash, Dot, DashDot, DashDotDot)

JSON fields:

Pen, Red, Green, Blue, Transparency, Width, Fading(True/False/nnn.n), LineStyle,
Enabled(True/False)

**http://localhost:7999/ToggleFading?P=n**

*Toggle Fading*

P = number -1(currentPen),0-9 , *mandatory*

JSON fields:

Pen, Fading

**http://localhost:7999/NextLineStyle?P=n**

*Select Next Line Style (Stroke -\> Solid -\> Dash -\> Dot -\> DashDot -\>
DashDotDot -\>Stroke)*

P = number -1(currentPen),0-9 , *mandatory*

JSON fields:

Pen, LineStyle

**http://localhost:7999/CurrentPen[?P=n]**

*Get/Set the selected Pen*

P = number 0-9, get only CurrentPen if ommited

JSON fields:

Pen (*currentPen)*

**http://localhost:7999/CurrentTool?T=nn[&F=nn] [&A=nn] [&I=aaaaa][&W=nnn]
[&H=nnn] [&D=nnn.n]**

*Get/Set the selected Tool and Filling*

T= (0=Hand, 1=Line, 2=Rect, 3=Oval, 4=Arrow, 6=Numbering, 7=Edit, 8=Text Left
Aligned, 9=Text Right Aligned, 10=Move, 11=Copy, 12=Resize, 13=Rotate,
21=PolyLine,  
22=ClipArt, 23=PatternOnStroke, -1=Erase, -2=Pointer, -3=Pan,-4=Lasso),
*mandatory*

F= (-1="NoFrame", 0="Empty", 1="Pen Colored", 2="White Colored", 3="Black
Colored") ; note -1 applies only to ClipArt, for the other it means "next
filling"; F is optional(-1 if omitted)

A=index of the Arrow Definition(Starting at 1)

I= Image Name(without extension) or Full Pathed Filename; optional if omitted
will open Selection Box only applies to Clipart and PatternOnStroke

W and H provides Image Size in Pixels, (-1 if omitted), only applies to Clipart
and PatternOnStroke  
for Clipart : -1 is the default size (used in simple click) is the File Image
size,  
for PatternOnStroke : -1 is the operator defined(1st step during drawing
procedure),

D provides Inter Images Distance in HiMetrics(-1 if omitted), only applies to
PatternOnStroke  
for PatternOnStroke : -1 is the operator defined(2nd step during drawing
procedure),

JSON fields:

Tool, ToolInText, Filling, FillingInText,Image(only for clipart) (InText are to
ease readability)

**http://localhost:7999/EnlargePen[?D=+nn]**

*Increment/decrement current Pen Width*

D = Delta value to apply to width (+/-) ; Optional(if not provided, will just
return current width)

JSON fields:

Width

**http://localhost:7999/Magnet[?M=True\|False]**

*Enable/Disable Magnetic Effect*

M=True/False ; optional (if not provided, will just return current magnetic
status)

JSON fields:

Magnet

**http://localhost:7999/Resize?K=nnn.n**

*Resize the current selection (from lasso) or hovered stroke (raise an error 500
if none is applicable)*

K= Scale factor (\>1 enlarge, \<1 reduce) *Mandatory*

JSON fields :

Results = OK (error 500 raised if any trouble)

**http://localhost:7999/Rotate?A=nnn.n**

*Rotate the current selection (from lasso) or hovered stroke (raise an error 500
if none is applicable)*

A= Angle (in degrees, positive=clockwise) *Mandatory*

JSON fields :

Results = OK (error 500 raised if any trouble)

**http://localhost:7999/GetSelection?[C][&L]**

*Returns Number of strokes, and/or length of the selection or Hover depending on
parameters*

C = will return number of strokes

L = will return total length of strokes

JSON fields :

Type (Selection/Hover), [Count], [TotalLength]

  
**http://localhost:7999/Magnify[?Z=No\|Dyn\|Capt]**

*Set the Zoom Mode*

Z= No/Dyn/Capt/Spot; optional (if not provided, will just return current Zoom)

JSON fields

Zoom

**http://localhost:7999/VisibleInk[?V=True\|False]**

*Set / Clear Visibility (default visible)*

V=True/False ; optional (if not provided, will just return current visibility
Status)

JSON fields:

VisibleInk

**http://localhost:7999/ClearScreen[?B=Tr\|Wh\|Bk\|Cu\|Me]**

*Clear all strokes*

B = Delete all strokes and set background (Tr=Transparent, Wh=White, Bk=Black,
Cu=Customed Color, Me=Open selection menu), empty is default setup...)

JSON fields:

"OK":true

**http://localhost:7999/Snapshot[?A=Out\|End\|Cont]**

*Clear all strokes*

Out = get out SnapShot

End=End Inking iaw config.ini

Cont=Carry On After(long press)

JSON fields:

"OK" : true

**http://localhost:7999/LoadStrokes[?F=aaaaa]**

*Load Stroke file*

F= filename ; -(dash) = open file dialog

JSON field:

"OK":true

**http://localhost:7999/SaveStrokes[?F=aaaaa]**

*Save Stroke file*

F= filename ; -(dash) = open file dialog

JSON field:

"OK":true

**http://localhost:7999/ArrowCursor[?A=True\|False]**

*Force the Arrow Cursor (True) or standard (False) ; quite similar to press Alt*

A=True\|False, empty returns only current status

JSon field

ArrowForced

**http://localhost:7999/Fold[?F=True\|False]**

*Fold/Dock or Unfold/Undock the toolbar or get its status*

F=True/False ; optional (if not provided, will just return current fold status)

JSON fields:

Folded

**http://localhost:7999/PickupColor[?P=True\|False]**

*Start/Exit Pickup Color Mode*

P=True/False ; optional (if not provided, will just return current Pickup mode
status/color)

JSON fields:

PickupMode(True/False),

Red(0-255),Green(0-255),Blue(0-255),Transparency(0-255) (Optionals, only if
pickupMode=true)

**http://localhost:7999/ChangePage [?N=-1/1]**

*Change Drawing Page*

N=-1 to simulate press on PagePrev ; N=1 to simulate press on PageNext

JSon fields :

PageNumber : current page starting at 1 (after change)

TotalPage : number of pages
