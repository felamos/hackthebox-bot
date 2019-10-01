FROM microsoft/dotnet:2.2-runtime-alpine

WORKDIR /app

COPY . .

CMD dotnet HackTheBox.dll
