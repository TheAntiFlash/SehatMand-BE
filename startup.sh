#!/bin/bash
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
./aws/install
aws --version

aws configure set aws_access_key_id $AWS_ACCESS_KEY_ID --profile sehatmand-profile
aws configure set aws_secret_access_key $AWS_SECRET_ACCESS_KEY --profile sehatmand-profile
aws configure set region $AWS_DEFAULT_REGION --profile sehatmand-profile
dotnet /home/site/wwwroot/SehatMand.API.dll
