@echo off
call config.bat

cd /D %MongoDir%
mongo --eval "db.adminCommand({shutdown : 1, force : true})" --port 27018
mongo admin --eval "db.shutdownServer()" --port 27017
mongo admin --eval "db.shutdownServer()" --port 27019
mongo admin --eval "db.shutdownServer()" --port 27020