@echo off

net stop Rollem.TaskRunnerService

"%~dp0Rollem.TaskRunnerService.exe" uninstall

pause