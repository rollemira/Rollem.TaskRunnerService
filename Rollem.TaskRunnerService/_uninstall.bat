@echo off

net stop RollemTaskRunnerService

"%~dp0Rollem.TaskRunnerService.exe" uninstall

pause