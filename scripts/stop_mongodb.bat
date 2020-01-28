cd  C:\Program Files\MongoDB\Server\3.6\bin
c:
mongo --eval "db.adminCommand({shutdown : 1, force : true})" --port 27018
mongo admin --eval "db.shutdownServer()" --port 27017
mongo admin --eval "db.shutdownServer()" --port 27019
mongo admin --eval "db.shutdownServer()" --port 27020