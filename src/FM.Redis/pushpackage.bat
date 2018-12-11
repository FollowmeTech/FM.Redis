echo on
echo please add nuget apikey and server to environment variables

set nuget=nuget.exe

for /f "tokens=*" %%i in ('dir *.nupkg /b') do call:publish %%i

:publish
%nuget% push %1 %nuget_apikey% -Source %nuget_server%

goto :eof
