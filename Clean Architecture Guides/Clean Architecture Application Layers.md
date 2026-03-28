### Domain Layer

- The core of you application
- Should encompass the core business logic and rules of you application.

#### Things you may see in the Domain Layer
- Entites
- Value objects
- Domain events
- Domain services
- Interfaces - the various abstractions needed in the domain layer.
- Custom exceptions
- Enums

### Application Layer

- Orchestrates the Domain layer
- Contains business logic that doesn't fit in the Domain Layer
- Defines the Use Cases - Use cases tell your domain entities what to do. You can do this through several approaches:
	- Application Services
	- CQRS with MediatR

	
**The Domain and Application layers make up the `Core` of the application**

### Infrastructure Layer
- External Systems 
	- Databases
	- Messaging
	- Email Providers
	- Storage Services
	- Identity
	- System Clock

### Presentation Layer
- Defines the entry point to the system
- Responsible for passing the requests to the layers below
- One example is a REST API 
