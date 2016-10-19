@echo off
REM ConvertXliff.bat - 18-Oct-2016 Greg Trihus
set myProg=\SIL\XliffTmxMd\XliffTmxMd.exe
set progDir=C:\Program Files
if exist "%progDir%%myProg%" goto foundIt
set progDir=%ProgramFiles(x86)%
if exist "%progDir%%myProg%" goto foundIt
set progDir=%ProgramFiles%
if exist "%progDir%%myProg%" goto fountIt
echo XliffTmxMd.exe not found
goto done

:foundIt
@echo on
"%progDir%%myProg%" -v %1
@echo off
:done
pause