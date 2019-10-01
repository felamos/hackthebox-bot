#!/usr/bin/env bash

cd src/HackTheBox

dotnet publish -c Release
cp ./config.json ./bin/Release/netcoreapp2.2/publish

cd ../../
cp ./Dockerfile ./src/HackTheBox/bin/Release/netcoreapp2.2/publish

docker build -t $1 ./src/HackTheBox/bin/Release/netcoreapp2.2/publish

docker tag $1 registry.heroku.com/$1/worker
docker push registry.heroku.com/$1/worker
heroku container:release worker -a $1
# heroku restart -a $1
