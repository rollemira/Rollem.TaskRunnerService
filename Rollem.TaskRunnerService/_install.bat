@echo off

"%~dp0Rollem.TaskRunnerService.exe" install

net start WellworxPrintService

pause