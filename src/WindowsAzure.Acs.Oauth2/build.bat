@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

REM Build
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild WindowsAzure.Acs.Oauth2.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false

REM Package
mkdir Build
.nuget\nuget.exe pack "WindowsAzure.Acs.Oauth2\WindowsAzure.Acs.Oauth2.csproj" -symbols -o Build -p Configuration=%config% %version%
.nuget\nuget.exe pack "WindowsAzure.Acs.Oauth2.Client\WindowsAzure.Acs.Oauth2.Client.csproj" -symbols -o Build -p Configuration=%config% %version%
.nuget\nuget.exe pack "WindowsAzure.Acs.Oauth2.Client.WinRT\WindowsAzure.Acs.Oauth2.Client.WinRT.csproj" -symbols -o Build -p Configuration=%config% %version%