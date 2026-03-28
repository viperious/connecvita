1. **Ubiquitous language** - A common language used by all members of the development team to ensure clear and consistent communication about the domain.

2. **Domain** - The subject area to which the user applies a program, and it represents the sphere of knowledge and activity around which the *application logic* revolves.

3. **Domain Invariants** - business rules.

4. **Tactical Patterns** - a set of building blocks that allow us to model our domain in a way that resembles the problem space. These patterns include:
	- Entities
	- Value Objects
	- Aggregates
	- Domain Services
	- Factories
	- Repositories
	- Domain Events

6. **Tactical Design** - the process of modeling a domain based on the problem space.

7. **Strategic Design** - process of discussing the domain with the domain experts.

8. **Entity** - an object that has an identifier. This allows us to know if two entities are equal.

Example Entity:
```csharp
public class Participant
{
	public Guid Id { get; }
	public string NickName { get; private set; }
}
```

In order to compare two entities you will override the `Equals` and `GetHashCode` methods in a base class as seen below:

```csharp
public abstract class Entity<T>
{
   public T Id {get; init;}

   protected Entity(T id)
   {
	 Id = id;
   }
   public override bool Equals(object? obj)
   {
	   if(obj is null || obj.GetType() != GetType())
	   {
		   return false;
	   }

	   return ((Entity)obj).Id == Id;
   }

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
}
```

9. Value Object - a type of object that represents something in the domain and is identified solely by its attributes and properties.
	- It's immutable
	- We know that if one value object's properties are equal to another value objects properties we're talking about the same property.
	- It's always part of an entity.

```csharp
public abstract class ValueObject
{
	public abstract IEnumerable<object?> GetEqualityComponents();

	public override bool Equals(object? obj)
	{
		if(obj is null || obj.GetType() != GetType())
		{
			return false;
		}

		return ((ValueObject)obj).GetEqualityComponents()
			.SequenceEqual(GetEqualityComponents());
	}

	public override int GetHashCode()
	{
		return GetEqualityComponents()
		{
			return GetEqualityComponents()
				.Select(x => x?.GetHashCode() ?? 0)
				.Aggregate((x,y) => x ^ y);
		}
	}
}
```

```csharp
public class TimeRange : ValueObject
{
	public TimeOnly Start { get; init; }
	public TimeOnly End { get; init; }

	public override IEnumerable<object> GetEqualityComponents()
	{
		yield return Start;
		yield return End;
	}
}
```

10. Aggregates - One or more domain objects that always need to stay consistent as a whole.

Below is an example of a method that adds a Session to a Participant's session id. The idea is if we try and add a session and there's an error, it will return an error. We then know that the session wasn't added and also that the aggregate is still valid. On the flip side, if we get a success, then we know that the session was added and the aggregate is in a valid state.

```csharp

public Result AddToSchedule(Session session)
{
	if(_sessionIds.Contains(session.id))
	{
		return Result.Error("Session already exists in the participant's schedule");
	}

	var bookTimeSlotResult = _schedule.BookTimeSlot(
		session.Date,
		session.Time);

	if(!bookTimeSlot.IsSuccess)
	{
		return Result.Errors("Cannot have two or more overlapping sessions");
	}

	_sessionIds.Add(session.Id);
	return Result.Success;
}
```

For every aggregate, there is an entity that acts as the root of the aggregate. If we shift our perspective from entities to aggregates, we will have a domain made of of aggregates where each one is self sufficient.

Aggregate Base Class:

```csharp
public abstract class AggregateRoot<T>: Entity<T>
{
	protected AggregateRoot(T id) : base(id)
	{
	}

	
}
```

Now that we have this class, we can restructure our project to have one folder for each Aggregate and its related entities. The naming convention should end with the suffix `Aggregate`. For example SessionAggregate.

11. Domain Services - a service class that contains business logic not suitable to be put inside of an entity.

Example:

```csharp

public class ReservationService
{
	public Result Reserve(List<Participants> participants)
	{
		//...
	}
}
```

This is a good example because each participant may have their own calendar/schedule it may be complicated to find time to book every participant. 

> [!Note]
> As much as possible, avoid using domain services in preference to putting the business logic inside of the entity itself. This prevents an ever growing, unmaintainable mess.

12. Factories - A class that helps use create complex objects.

```csharp
public class SessionFactory
{
	public static Session Create(
		string name,
		string description,
		//..
		Guid? id = null
	)
	{
		// .. complex creation logic
	}
}
```

An option instead of creating a separate class is to include the static `Create` method inside the entity itself. This is my preference.

13. Repository - a class that contains a collection of methods that manipulates an entity. The benefit is it abstracts away the underlying implementation. We don't need to know how it does it. All we need to know is we call the method and an entity is added, saved, fetched etc.