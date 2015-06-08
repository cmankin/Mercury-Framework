@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

:: set the correct path to the the app
set ttPath="%CommonProgramFiles(x86)%\Microsoft Shared\TextTemplating\1.2\TextTransform.exe"

:: set the working dir (default to current dir)
set wdir="%~dp0"
echo current working directory %wdir%

:: set the file extension (default to vb)
set extension=cs

echo executing 'buildT4' from %wdir%
:: create a list of all the T4 templates in the working dir
dir %wdir%\*.tt /b /s > t4list.txt

echo the following T4 templates will be transformed:
type t4list.txt

:: transform all the templates
for /f "tokens=*" %%d in (t4list.txt) do (
set file_name=%%d
echo: current file name= !file_name!
set file_name=!file_name:~0,-3!.%extension%
echo:  \--^> !file_name!   
%ttPath% -out "!file_name!" "%%d"
echo:  \--^> %ttPath% -out "!file_name!" "%%d"
)

echo transformation complete