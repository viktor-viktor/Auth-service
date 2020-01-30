@echo off

call config.bat

call stop_mongodb.bat

cd /D %ProjDir%/scripts

call stop_replica_set.bat

cd /D %MongoDir%
mongod --port 27017 --dbpath %MongoDBDir% --replSet rs0

pause