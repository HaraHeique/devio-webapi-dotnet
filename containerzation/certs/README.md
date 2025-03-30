## Para geração de certificado local

Gerar certificados serve para conseguir rodar a aplicação usando protocolo HTTPS juntamente com o docker compose em um ambiente configurado específico para isto, onde o nosso caso seria o `ASPNETCORE_ENVIRONMENT=Contaneirzation`.

Para isto basta abrir o terminal CLI no diretório que encontra este documente README e executar os seguintes comandos do DOTNET CLI:

```cli
dotnet dev-certs https -ep .\devio-api-certificate.pfx -p devio-api
dotnet dev-certs https --trust
```

Após isto basta rodar no Visual Studio o projeto do Docker Compose.