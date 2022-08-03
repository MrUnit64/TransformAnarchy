@echo OFF 
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" x64
echo "Building"
devenv "RotationAnarchy.sln" /build Release 
pause
cmd /k