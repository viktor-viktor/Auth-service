@echo off
config.bat

call start_replica_set.bat

cd /D %ProjDir%/..
dotnet run --launch-profile Prod