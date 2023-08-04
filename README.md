# Series Serverless Service

Serviço de gerenciamento de versões de séries de exercícios entre instrutor e aluno. Composto por um aplicativo android nativo e uma api sem servidor para gerenciamento de séries de exercícios entre instrutor e aluno.

- Series API with AWS Lambda and .NET, Amazon API Gateway, Amazon DynamoDB, and AWS SAM
- Android Native App with Kotlin

## Arquitetura

A arquitetura do aplicativo utiliza o Amazon API Gateway para se comunicar com uma função AWS Lambda escrita em .NET, que faz chamadas ao Amazon DynamoDB, com o AWS Identity and Access Management (IAM) fornecendo controle de acesso entre os componentes, conforme mostrado abaixo:

![Arquitetura serverless](https://github.com/aws-samples/aws-net-guides/raw/master/Serverless/Serverless%20App%20with%20Dynamo-Example/media/figure01.png)

## Implantação

A implantação dos componentes do aplicativo é gerenciada pelo AWS CloudFormation, que utiliza o AWS Serverless Application Model (AWS SAM) para simplificar o modelo.

O modelo do CloudFormation contém detalhes de configuração para cada componente e também faz referência a um bucket S3 usado para armazenar os artefatos de implantação, como o assembly .NET. Ao ser executado, o CloudFormation utiliza o modelo e o bucket S3 para criar um stack do CloudFormation que, em seguida, implanta os componentes do aplicativo.

![Implantação na nuvem](https://github.com/aws-samples/aws-net-guides/raw/master/Serverless/Serverless%20App%20with%20Dynamo-Example/media/figure02.png)

docker run -d -p 8000:8000 --network=local-acad-serverless-api-network --name dynamo-local amazon/dynamodb-local

sam local start-api --docker-network=local-acad-serverless-api-network



## Data Modeling

Single table design usign global secondary indexes and usign one to many relationships with secondary index plus query pattern to retrieve data. 

https://www.youtube.com/watch?v=BnDKD_Zv0og
## Access Patterns

- Buscar os dados de todos aluno :heavy_check_mark:
- Buscar os dados de um aluno :heavy_check_mark:
- Buscar todas as séries de um aluno :heavy_check_mark:
- Buscar todos os instrutores :heavy_check_mark: 
- Buscar os dados de um instrutor :heavy_check_mark: 
- Buscar todos os alunos de um instrutor :heavy_check_mark: 
- Buscar todos as séries de um instrutor por aluno :heavy_check_mark:

| Entity | PK | SK |
| ---- | --- | ---------- |
| Student | student#< studentId > | student#< studentId > |
| Serie | student#< studentId > | serie#< serieVersion >
| Instructor | instructor#< instructorId > | instructor#< instructorId > |
 
Global Secondary Indexes

## Ambiente de Desenvolvimento

Instruções para setup inicial do ambiente de desenvolvimento podem ser encontradas aqui https://aws.amazon.com/pt/blogs/dotnet/get-started-with-net-development-on-aws/

- Setup and configure your Amazon account
- Install and configure the AWS CLI https://docs.aws.amazon.com/cli/latest/userguide/cli-chap-configure.html
- Install the AWS Extensions for .NET CLI https://github.com/aws/aws-extensions-for-dotnet-cli
- Install the AWS.NET deployment tool for .NET CLI https://github.com/aws/aws-dotnet-deploy#readme
- Install and Configure AWS IDE Toolkits https://aws.amazon.com/developer/language/net/tools/?refid=ha_awssm-1524
- Install the AWS Toolkit for Visual Studio https://marketplace.visualstudio.com/items?itemName=AmazonWebServices.AWSToolkitforVisualStudio2022
- Install the .NET 7.0 SDK to use the native AOT functionality https://dotnet.microsoft.com/en-us/download/dotnet/7.0
- Install the AWS SAM CLI tool https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html
- Docker Desktop configured to run Linux containers https://www.docker.com/products/docker-desktop/

## Resources

- Event-driven .NET applications with AWS Lambda and Amazon EventBridge https://aws.amazon.com/pt/blogs/dotnet/event-driven-net-applications-with-aws-lambda-and-amazon-eventbridge/
- Building Serverless .NET Applications with AWS Lambda and the SAM CLI https://aws.amazon.com/pt/blogs/dotnet/building-serverless-net-applications-with-aws-lambda-and-the-sam-cli/
- Building serverless .NET applications on AWS Lambda using .NET 7 https://aws.amazon.com/pt/blogs/compute/building-serverless-net-applications-on-aws-lambda-using-net-7/
- Getting started with AWS SAM https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/serverless-getting-started.html
- AWS Serverless Application Model https://aws.amazon.com/pt/serverless/sam/
- How to run API Gateway, AWS Lambda and DynamoDB locally https://andmoredev.medium.com/how-to-run-api-gateway-aws-lambda-and-dynamodb-locally-91b75d9a54fe
- Amazon API Gateway with .NET – AWS Lambda & DynamoDB Integrations https://codewithmukesh.com/blog/amazon-api-gateway-with-dotnet/
- AWS Lambda with .NET 6 – Getting Started with Serverless Computing https://codewithmukesh.com/blog/aws-lambda-with-net-6/
- CRUD with DynamoDB in ASP.NET Core – Getting Started with AWS DynamoDB Simplified https://codewithmukesh.com/blog/crud-with-dynamodb-in-aspnet-core/
- Build a Serverless app using API Gateway, Lambda and DynamoDB https://github.com/aws-samples/aws-net-guides/tree/master/Serverless/Serverless%20App%20with%20Dynamo-Example
