@echo off

call config.bat

call stop_mongodb.bat

cd /D %ProjDir%/scripts

call stop_replica_set.bat

cd /D %MongoDir%
mongod --config "%ProjDir%/scripts/local_deploy/mongo_replica_1.conf" --dbpath "%ProjDir%/../Mongo_DBs/Auth_DB/replica_1"
mongo --eval "rs.initiate()"
