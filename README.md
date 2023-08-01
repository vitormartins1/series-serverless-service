# acad-serverless


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