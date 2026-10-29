Microservices Assignment, the amazing Notes Application!

This is a small demonstration application for the course Applied Cloud Computing and Big Data course. 
It consists of two .NET 9 REST services and a Microsoft SQL Server database, deployed to a local Kubernetes cluster.

Prerequisites
- Access to the container images on Docker Hub
- A SQL client such as SQL Server Management Studio or sqlcmd for initial database setup

Commands below use PowerShell and should be executed in the repository root.

1. Create the Kubernetes Secret
The SQL Server administrator password is not stored in the Kubernetes YAML files. Create the Secret before deploying:
kubectl create secret generic sqlserver-secret `
    --from-literal=MSSQL_SA_PASSWORD='<choose-a-strong-password>'

2. Deploy the application
kubectl apply -f .\k8s\sqlserver.yaml
kubectl apply -f .\k8s\service-b.yaml
kubectl apply -f .\k8s\service-a.yaml

Check the resources:
kubectl get deployments
kubectl get pods
kubectl get services
kubectl get pvc

3. Initialize the database
The application expects a database named TestDb containing a table named dbo.Notes. The following is a reference schema; verify the column types and defaults against the actual Service B implementation before running it.
IF DB_ID(N'TestDb') IS NULL
BEGIN
    CREATE DATABASE TestDb;
END;
GO

USE TestDb;
GO

IF OBJECT_ID(N'dbo.Notes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notes
    (
        Id        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Note      NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;
GO

4. Verify the API
In the tested Docker Desktop environment, Service A is available at http://127.0.0.1:8080.

Read notes:
Invoke-RestMethod http://127.0.0.1:8080/Api/GetNotes

Insert a note
Invoke-RestMethod `
    -Uri "http://127.0.0.1:8080/Api/InsertNote" `
    -Method Post `
    -ContentType "application/json" `
    -Body '"Testing the insert 2"'

Happy noting!!

/Erik
