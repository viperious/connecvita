# Domain Project

### Abstractions Folder
This folder holds the following abstractions:
- The base `Enity` class and `AuditableEntity` abstract classes. Both these classes are generic allowing you to specify the Id type of the entity.
- The `IDomainEvent` interface is used when publishing domain events using MediatR's `INotification` interface.
- The `IUnitOfWork` interface is here in case you want to implement your own unit of work pattern when working with or without EF Core.
- The `ValueObject` abstract class can be used in place of the C# record type for implementing value objects.

> [!Note]
> It is suggested to use C# record types when implementing value objects.

### Folder Structure

It is suggested to group each entity and its related files into their own separate folder. These folder can include but are not limited to:
- Events
- Exceptions
- Value Objects