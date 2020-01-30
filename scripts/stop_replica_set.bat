@echo off
call config.bat

cd /D %MongoDir%
mongo --eval "db.adminCommand({shutdown : 1, force : true})"
