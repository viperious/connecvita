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

### Demo Core Entities Conceptual Model

![[Domain Layer - Demo Core Entities.png]]

### Translating the conceptual model to core entities

How to structure entities in Domain project.

1. Create a folder for each feature and place the entities related to that feature in the respective folder.
	- For this example, create a folder called `Apartments`
	- Create a class called `Apartment` inside of the folder. This will represent the `Apartment` entity. ***If you don't plan on inheriting this class it's good practice to make this class sealed***
	- Add the properties to the class
			-This is the starting class (will be modified as we move along)
			
		```csharp
		public sealed class Apartment 
		{
			public GUID Id {get; private set;}
			public string Name {get; private set;}
			public string Description {get; private set;}
			public string Country {get; private set;}
			public string State {get; private set;}
			public string ZipCode {get; private set;}
			public string City {get; private set;}
			public string Street {get;private set;}
			public decimal PriceAmount {get; private set;}
			public string PriceCurrency {get; private set;}
			public decimal CleaningFeeAmount {get; private set;}
			public string CleaningFeeCurrency {get; private set;}
			public DateTime? LastBookedOnUtc {get; private set;}
			public List<Amenity> Amenities {get; private set;} = new();
			
		}
```

The `Apartment` class can have one or more amenities. To model this we'll use an Enum called `Amenity`.
```csharp
public enum Amenity 
{
	WiFi = 1,
	AirConditioning = 2,
	Parking = 3,
	PetFriendly = 4,
	Swimming Pool = 5,
	Gym = 6,
	Spa = 7,
	Terrace = 8,
	MountainView = 9,
	GardenView = 10
}
```

The above model is what is referred to as an ***Anemic*** model. This means it only acts as a bag of data with a bunch properties and no behavior. We will want to refactor the above class into a rich domain model using principles from DDD.

### What is an Entity

An object in your domain that satisfies two things:
1. It has an identity, it's unique
2. It continuous, it's important throughout the lifetime of the application. Since it has an Id, it can evolve and change over time.

Since an entity is important to our application we'll create an abstraction for the entity type.
- Create a folder called `Abstractions` and add the following abstract class:
- 
	```csharp
	public abstract class Entity 
	{
		protected Entity(Guid id){
		   Id = id;
		}
		
		public Guid Id {get; init; }
	}
```

We're using `Guid` type for the id, however if you need to use other types I would create a generic entity like this:

```csharp
	public abstract class Entity<T>
	{
		protected Entity(T id){
		   Id = id;
		}
		
		public T Id {get; init; }
	}
```

We're using the `init` setter because once the entity is created, the id is set for life and can't change. 

If comparing two entities is important to your application, you can override the `Equals` method and implement the `IEquatable` interface.

Now we can update our `Apartment` class to look like this:

```csharp
public sealed class Apartment : Entity
{
	public Apartment(Guid id): base(id){};
	
	public string Name {get; private set;}
	public string Description {get; private set;}
	public string Country {get; private set;}
	public string State {get; private set;}
	public string ZipCode {get; private set;}
	public string City {get; private set;}
	public string Street {get;private set;}
	public decimal PriceAmount {get; private set;}
	public string PriceCurrency {get; private set;}
	public decimal CleaningFeeAmount {get; private set;}
	public string CleaningFeeCurrency {get; private set;}
	public DateTime? LastBookedOnUtc {get; private set;}
	public List<Amenity> Amenities {get; private set;} = new();
			
}
```

When we use this approach, we can easily identity all the entities in our application.

### Value Objects

Value objects are used to solve primitive obsession, when code relies too much on primitives. The problem with primitive types is they don't convey any meaning. Value objects help to fix this problem. They also improve the design of the application.

We will use a value object to encapsulate the address information in the `Apartment` class.

#### How to implement a value object

In C# the best approach for value objects is using a `Record` type. To understand why we should use a record we need to define what a value object is.

A value object is an object that:
- Is uniquely identified by its values, i.e. structural equality.
- Immutability

A record type does both making it the perfect candidate for value objects.

```csharp
public record Address(
	string Country,
	string State,
	string ZipCode,
	string City,
	string Street
)
```

We can now replace the address information with this value object.

```csharp
public sealed class Apartment : Entity
{
	public Apartment(Guid id): base(id){};
	
	public string Name {get; private set;}
	public string Description {get; private set;}
	public Address {get; private set;}
	public decimal PriceAmount {get; private set;}
	public string PriceCurrency {get; private set;}
	public decimal CleaningFeeAmount {get; private set;}
	public string CleaningFeeCurrency {get; private set;}
	public DateTime? LastBookedOnUtc {get; private set;}
	public List<Amenity> Amenities {get; private set;} = new();
			
}
```

Any time you see a string in a class, it's a good time to ask yourself if that property would be better off as a value object.

Let's add a few more including the idea of Price and Currency. Both are important in this context so lets create two more value objects related to money.

```csharp
public record Currency
{
	// This is internal because we're not going to return it as an available
	// type
	internal static readonly Currency None = new("");
	public static readonly Currency Usd = new("USD");
	public static readonly Eur = new("EUR");

	private Currency(string code) => Code = code;
	
	public string Code {get; init;}

	// This method allows a user to pass in a currency code and get a new Currency
	// object

	public static Currency FromCode(string code)
	{
		return All.FirstOrDefault(c => c.Code == code) ??
				throw new ApplicationException("The currency code is invalid")j;
	}
	// This method allows you to see all available currencies
	public static readonly IReadOnlyCollection<Currency> All = new[]
	{
		Usd,
		Eur
	}
}
```

As you can see there's more involved in a currency object that just a string. The code above allows us to constrain the behavior of our code and prevent invalid state.

Now let's create the `Money` value object:

better off as a value object.

Let's add a few more including the idea of Price and Currency. Both are important in this context so lets create two more value objects related to money.

```csharp
public record Money(decimal Amount, Currency currency)
{
	// We can add useful functions for working with money in our value object
	public static Money operator +(Money first, Money second){
		if(first.Currency != second.Currency){
			throw new InvalidOperationException("Currencies have to be equal");
		}
		return new Money(first.Amount + second.Amount, first.Currency);
	}

	// We can also represent a money object with no value
	public static Money Zero() => new(0, Currency.None);
}

```

Now we can update our `Apartment` class with value objects

>[!Note]
>The Name value object wasn't created above but it would exist if we were building this 

```csharp
public sealed class Apartment : Entity
{
	public Apartment(Guid id): base(id){};
	
	public Name Name {get; private set;}
	public string Description {get; private set;}
	public Address {get; private set;}	
	public Money Price {get; private set;}
	public Money CleaningFee {get; private set;}	
	public DateTime? LastBookedOnUtc {get; private set;}
	public List<Amenity> Amenities {get; private set;} = new();
			
}
```

***The importance of private setters in the domain model and encapsulation***

We don't want to allow the values of the entities to be changed from outside of the entity. If we want the values to be updatable, we should implement a function to do so. This allows us to maintain the entity in a valid state.

#### Using the Static Factory Pattern

We will show how to use the static factory pattern to create an entity using the `User` class.

First we want to create a folder called Users to hold anything related to the `User` type. Next we will create a class that looks like this:

> [!Note]
> We won't implement any value objects for the User class here as it is just a look at the static factory pattern

```csharp
public sealed class User : Entity 
{
	private User(Guid id,
			    FirstName firstName,
			    LastName lastName,
			    Email email): base(id)
	{
		FirstName = firstName;
		LastName = lastName;
		Email = email;			    
    }

	public FirstName FirstName {get; private set;}
	public LastName LastName {get;private set;}
	public Email Email {get;private set;}

	// Here is the implementation of the factory 
	public static User Create(FirstName firstName, LastName lastName, Email email)
	{
		var user = new User(Guid.NewGuid(), firstName, lastName, email);

		// This will raise the domain event. The relevant code is created below

		user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));
		
	    return user;
	}
}
```

So what's the point of using a static factory method. It allows us to:
- You're hiding your implementation details.
- Encapsulation
- You can introduce side effects, for example, Domain Events that don't belong in a constructor.


#### Domain Events

Domain events are important events that occur in our system that you want other parts of the same domain to be aware of. To implement the domain event pattern do the following:

1. We will use the `MediatR` nuget package to implement our domain events. So add `MediatR.Contracts` to our domain project. This package contains all the necessary contracts needed.

2. Add the `IDomainEvent` interface that implements the `INotification` interface from the `MediatR` package  in the `Abstractions` folder with the following code:
	 ```csharp
	public interface IDomainEvent : INotification
	{
		
	}
```

3. Next we will update the `Entity` base class to look like this:

```csharp
public abstract class Entity 
	{
		private readonly List<IDomainEvent> _domainEvents = new();
		
		protected Entity(Guid id){
		   Id = id;
		}
		
		public Guid Id {get; init; }

		// The methods below are necessary to implement the domain events
		public IReadOnlyList<IDomainEvent> GetDomainEvents();
		{
			return _domainEvents.ToList();
		}

		public void ClearDomainEvents(){
			_domainEvents.Clear();
			
		}

		protected void RaiseDomainEvent(IDomainEvent domainEvent){
			_domainEvents.Add(domainEvent);
		}
	}
```

4. Next create a folder in the `Users` folder called `Events` which will hold all the Domain Events relevant to the `User`.

5. Add your first event. This should be a `Record` type.
```csharp
public record UserCreatedComainEvent(Guid userId) : IDomainEvent;
```

***Now when a user is created, other parts of the application can subscribe to this event and react accordingly when a new user is created. This is also known as the pub/sub model***


#### Repositories and the Unit of Work pattern

We will use the repository pattern. A repository should exist for every entity. This ensures that the repository doesn't grow out of control and is focused on one thing (The single responsibility of SOLID).

1. Create the `IUsersRepository` abstraction inside of the `Users` folder. 
```csharp
public interface IUserRepository
{
	Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

	void Add(User user);
}
```

So how do we persist the changes. In this case we will use the Unit of Work pattern. This will allow us to do many things, the top thing is to be persistent ignorant.

1.  Add the `IUnitOfWork` interface inside of the `Abstractions` folder.
```csharp
public interface IUnitOfWork
{
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

>[!Note]
>Repositories should operate only on Domain Entities. Therefore we could refer to the repositories as Entity repositories.


### Alternatives to using the DateTime Type

Instead of using the DateTime type to represent a Date, it can be useful to abstract that away into a value object. This will allow for easier testing and better encapsulation when comparing two date objects. In this example we will create a `DateRange` value object which will allow use to encapsulate the range of when an Apartment can be booked. This will also help us avoid overlapping bookings. To accomplish this create the following value object:

```csharp
public record DateRange
{
	private DateRange(){}

	public DateOnly Start {get; init; }
	public DateOnly End {get; init;}

	public int LengthInDays => End.DayNumber - Start.DayNumber;

	public static DateRange Create(DateOnly start, DateOnly end)
	{
		if(start > end){
			throw new ApplicationException("End date precedes start date");
		}

		return new DateRange
		{
			Start = start,
			End = end
		};
	}
}
```


#### Implementing a Domain Service

A domain service is used to implement logic that doesn't fit well inside of an entity. An example of this is pricing logic when creating a booking.

1. Create a new class (inside of the feature folder) in this case inside the `Bookings` folder called `PricingService`.

```csharp
public class PricingService 
{

	// Add all necessary logic here

}
```


> [!Note]
> If you find yourself needing to access any classes in multiple feature folders, it's a good idea to create a `Shared` folder and move the classes there


### The Result Pattern

The result pattern allows you to return a success/fail type indication instead of a void or boolean or int. This makes the logic in your application much cleaner. I prefer to use the `Ardalis.Result` nuget package to implement the pattern, but you can implement your on Result class if you'd like.

### Summary

The final folder structure of our Domain project should look something like this

| -- **Abstractions**
|	 - IUnitOfWork
|	 - Entity
|	 - IDomainEvents
|
| -- **Apartments (Feature Folder)**
|	- Apartment (Entity)
|	- IApartmentRepository (Repository Abstraction)
|	- Description (Value Object)
|	- Name (Value Object)
|	- Address (Value Object)
|	- Amenity (Enum)
|
| -- **Bookings (Feature Folder)**
|   - PricingService (Domain Service)
|   - Booking (Entity)
|   - DateRange (Value Object)
|   - IBookingRepository (Repository Abstraction)
|   - **Events (Domain Events)**
|      - BookingReservedDomainEvent (Domain Event)
|      - BookingCancelledDomainEvent (Domain Event)
|      - BookingCompletedDomainEvent (Domain Event)
|      - BookingConfirmedDomainEvent (Domain Event)
|
| -- **Shared**
|   - Money (Value Object)
|   - Currency (Value Object)