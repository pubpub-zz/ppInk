cd /D %~dp0
cd
ren %windir%\assembly\GAC_64\Microsoft.Ink\6.1.0.0__31bf3856ad364e35\microsoft.ink.dll microsoft.ink.dll.sav
copy microsoft.ink.dll %windir%\assembly\GAC_64\Microsoft.Ink\6.1.0.0__31bf3856ad364e35\microsoft.ink.dll
pause
