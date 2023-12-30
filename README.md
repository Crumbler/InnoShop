# InnoShop

This project contains 2 microservices: [UserService](ProductService) and [ProductService](ProductService).
The project was created as a test task for a certain company.

Both microservices were developed using Onion Architecture and use Microsoft SQL Server Express as the database.

Authentication and authorization are handled via JWT tokens:
- The "sub_id" claim contains the user's id
- The "admin" claim indicates whether the user has admin privileges or not.

The encryption algorithm used is RSA.
Due to it being an asymmetric algorithm, only the Users service needs the private key and all the other services only require the public key.

The public and private keys were generated with the following commands:

```
ssh-keygen -t rsa -b 1024 -m pem -f privateKey.txt
ssh-keygen -t rsa -b 1024 -m pem -f privateKey.txt -e > publicKey.txt
```

Libraries used:
- [BCrypt.Net](https://github.com/BcryptNet/bcrypt.net) for hashing and verifying passwords.
- [MailKit](https://github.com/jstedfast/MailKit) for sending emails.
- Entity Framework Core as the ORM.
- [SmtpServer](https://github.com/cosullivan/SmtpServer) for a simple SMTP server.
- [LINQKit](https://github.com/scottksmith95/LINQKit) for building predicates that are used when searching and filtering products.
- [NUnit](https://github.com/nunit/nunit) and [moq](https://github.com/devlooped/moq) for testing.

Users can have one of 2 roles: Regular and Admin:

- Regular users are allowed to edit and delete their accounts and products.
- Users with admin privileges are able to edit and delete other users and their products.

[SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) is required for running the microservices under the "Development" profile and the integration tests.

When running the Users service by itself or as part of Docker Compose you need a local SMTP server if you want to receive email confirmation and password reset emails.
You can use the [EmailServer](EmailServer) project or any other local SMTP server.

When using the Test Explorer in Visual Studio make sure to use the "Testing" Solution configuration.
Pressing "Run All Test In View" causes Visual Studio to build every project, which can lead to build errors if Docker is not launched (because of the docker-compose file).

For interaction with the services external tools such as cURL and Postman are recommended.
