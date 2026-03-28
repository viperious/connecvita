The application layer sits one level above the domain layer and is allowed to reference it, but is still part of the application core. It has no dependencies on external resources, but if needed there can be an implementation to access outside resources in this layer. Trade offs and implementation details will be discussed below. 

Overall the following is found in the application layer:

![[Application Layer overview.png]]

### What is CQRS (Command Query Responsibility Segregation)

![[Benefits of CQRS.png]]

It splits the responsibilities of writing the data from reading the data. This allows us to optimize the read database from our write database if speed is a requirement.

#### CQRS Pattern Overview

![[CQRS pattern overview.png]]

#### Configuring the Application Layer dependencies

1. Install the `MediatR` nuget package.
2. Create a new class at the root of the project called `DependencyInjection` this will be the class responsible for wiring up all the dependencies in the Application layer.
```csharp
public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddMediatR(configuration => {
			configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);		
		});
		
		return services;
	}
}
```


#### Adding the required abstractions for CQRS

1. Add an `Abstractions` folder at the root of the project.
2. Inside of that folder add a folder called `Messaging`.
3. Inside of this folder you will define your command and query abstractions. Add the following interfaces:

```csharp

// Query interface
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
	
}

// Query handler interface
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>
{

}
```

Both of these implement MediatR's `IRequest` and `IRequestHandler` interfaces and return an "Envelope" response, that is the Result pattern.

```csharp

// Command interfaces
public interface ICommand : IRequest<Result>, IBaseCommand
{

}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand
{

}

// Base Command interface implemented by both commands. This is a useful contraint when using pipeline behaviors

public interface IBaseCommand 
{

}

// Command handlers

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result> where TCommand : ICommand 
{

}

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>> where TCommand : ICommand<TResponse>
{

}

```

The command interfaces follow the same principles as the query interfaces.

#### Implementing Commands with the rich domain model & repositories

1. Create a feature folder called `Bookings` at the root of the project. This will hold all the commands and queries related to bookings.
2. Create the first feature, aka use case, for bookings. Create a folder called `ReserveBooking` and add a record that will be the command:
```csharp

// Command with required parameters
public record ReserveBookingCommand(
 Guid ApartmentId,
 Guid UserId,
 DateOnly StartDate,
 DateOnly EndDate
) : ICommand<Guid>;

// Command handler for the ReserveBookingCommand

internal sealed class ReserveBookingCommandHandler : ICommandHandler<ReserveBookingCommand, Guid>
{
	private readonly IUserRepository _userRepository;
	private readonly IApartmentRespository _apartmentRepository;
	private readonly IBookingRepository _bookingRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly PricingService _pricingService;
	
	public ReserveBookingCommandHandler(
		IUserRepository userRepository, 
		IApartmentRepository apartmentRepository, 
		IBookingRepository bookingRepository, 
		IUnitOfWork unitOfWork, 
		PricingService pricingService)
	{
		_userRepository = userRepository;
		_apartmentRepository = apartmentRepository;
		_bookingRepository = bookingRepository;
		_unitOfWork = unitOfWork;
		_pricingService = pricingService;
	}
	
	public async Task<Result<Guid>> Handle(ReserveBookingCommand request, CancellationToken cancellationToken)
	{
		var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

		if(user is null){
			return Result.Failure("User not found");
		}

		var apartment = await _apartmentRepository.GetByIdAsync(request.ApartmentId, cancellationToken);

		if(apartment is null){
			return Result.Failure("Apartment not found");
		}

		var duration = DateRange.Create(request.StartDate, request.EndDate);

		if(await _bookingRepository.IsOverlappingAsync(apartment, duration,cancellationToken)){
			return Result.Failure("There is an overlap in booking request");
		}

		var booking = Booking.Reserve(
			apartment,
			user.Id,
			duration,
			utcNow: DateTime.UtcNow,
			_pricingService);

		_bookingRepository.Add(booking);
		
	// this introduces a race condition which will be solved later
		await _unitOfWork.SaveChangesAsync(cancellationToken);
	}

		return booking.Id;
}
```


### Useful Abstraction for DateTime.Now

1. Create a folder in the `Abstractions` folder called `Clock`
2. Add the following interface:
```csharp
public interface IDateTimeProvicer
{
	DateTime UtcNow {get;}
}
```

What this allows us to do is use different date time provider implementations depending on the use case, for example creating a testing `IDateTimeProvider` which allows us to mock the date time making our code testable.

#### Defining a Domain Event Handler

When adding a domain event handler, it's a good practice to place it in the feature folder which implements it. In this case, the `BookingReservedDomainEventHandler` should be placed in the Bookings -> ReserveBooking folder.

```csharp
internal sealed class BookingReservedDomainEventHandler : INotificationHandler<BookingReservedDomainEvent>
{

	private readonly IBookingRepository _bookingRepository;
	private readonly IUserRepository _userRepository;
	private readonly IEmailService _emailService;
	
	public BookingReservedDomainEventHandler(
		IBookingRepository bookingRepository,
		IUserRepository userRepository,
		IEmailSservice emailService	
	)
	{
		_bookingRepository = bookingRepository;
		_userRepository = userRepository;
		_emailService = emailService;
	}
	
	public async Task Handle(BookingReservedDomainEvent notification, CancellationToken cancellationToken)
	{
		var booking = await _bookingRepository.GetByIdAsync(notification.BookingId, cancellationToken);

	if(booking is null)
	{
		return;
	}

	var user = await _userRepository.GetByIdAsync(booking.UserId, cancellationToken);

	if(user is null)
	{
		return;	
	}

	await _emailService.SendAsync(
		user.Email,
		"Booking reserved!",
		"You have 10 minutes to confirm this booking"
	);
	}
}
```

> [!Note]
> The approach above, using repositories, isn't always the most performant way to implement queries, commands, or domain events. Using Dapper directly from the domain layer breaks the rule that external resources are access in the `Infrastructure` layer. However, depending on your needs this may be ok


### Solving Cross-cutting concerns - Logging

A good way to log across different projects is using MediatR's pipeline behavior.

1. Inside of the `Abstractions` folder create a new folder called `Behaviors`.
2. Install the `Microsoft.Extensions.Logging.Abstractions` nuget package.
3. Add the following:
```csharp

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IBaseCommand
{

private readonly ILogger<TRequest> _logger;

public LoggingBehavior(ILogger<TRequest> logger)
{
	_logger = logger;
}
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var name = request.GetType().Name;
	try
	{
		_logger.LogInformation("Executing command {Command}", name);
		var result = await next();

		_logger.LogInformation("Command {Command} processed successfully", name);

		return result;
	}
	catch(Exception exception)
	{
		_logger.LogError(exception, "Command {Command} processing failed",name);
		throw;
	}
	}
}
```

So why IBaseCommand? The reason is we only want logging running on our Commands. We don't include queries unless there is a specific use case. By not including queries, they will run as fast as possible.

4. Now register the pipeline behavior in the `DependencyInjection` class.
```csharp
public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddMediatR(configuration => {		
		configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);	
		configuration.AddOpenBehavior(typeOf(LoggingBehavior<,>));
				
		});
		
		return services;
	}
}
```



### Solving Cross-cutting concerns - Validation

1. Install the `FluentValidation.DependencyInjectionExtensions` nuget package.
2.  Add the `ValidationBehavior` class inside of the `Behaviors` folder.
3. Add the following:
```csharp

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IBaseCommand
{

private readonly IEnumerable<IValidator<TRequest>> _validators;

public ValidationBehavior(IEnumerable<IValidator<TRequest> validators)
{
	_validators = validators;
}
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
 {
		if(!_validators.Any()){
			return await next();
		}

		var context = new ValidationContext<TRequest>(request);

		var validationErrors = _validators
			.Select(validator => validator.Validate(context))
			.Where(validationResult => validationresult.Errors.Any())
			.SelectMany(validationResult => validationResult.Errors)
			.Select(valicationFailure => new ValidationError(
				validationFailure.PropertyName,
				validationFailure.ErrorMessage
			)).ToList();

	if(validationErrors.Any())\
	{
		throw new Exceptions.ValidationException(validationErrors):	
	}

	return await next();
  }
}

// custom validation error record
public sealed record ValidationErrors(string PropertyName, string ErrorMessage);

// custom validation exceptions class

public sealed class ValidationException : Exception
{
	public ValidationException(IEnumerable<ValidationError> errors)
	{
		Errors = errors;
	}

	public IEnumerable<ValidationError> Errors {get;}
}

```


> [!Note]
> The custom exceptions should be located in a folder located at the root of the project called `Exceptions`.


> [!Note]
> We will handle any exceptions thrown in another layer of the application, for example the presentation layer

4. Now register the Validator in the `DependencyInjection` class as seen below:
```csharp
public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddMediatR(configuration => {		
		configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);	
		configuration.AddOpenBehavior(typeOf(LoggingBehavior<,>));
		configuration.AddOpenBehanvior(typeOf(ValiationBehanvior<,>));
				
		});
		
		return services;
	}
}
```

5. Finally add any validators needed. They should be co-located in a feature folder. We will use the ReserveBookingCommand as example:
```csharp
public class ReserveBookiungCommandValidator : AbstractValidator<ReserveBookingCommand>
{
	public ReserveBookingCommandValidator()
	{
		// These are just a few examples of rules. for more information.
		RuleFor(c => c.UserId).NotEmpty();
	}
}
```

For more information on how to use FluentValiation see  See [FluentValidation — FluentValidation documentation](https://docs.fluentvalidation.net/en/latest/)

6.  Register the validators with the `DependencyInjection` class:

```csharp
public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
	
	//....
	
	services.AddValidatorsFromAssembly(typeOf(DependencyInjection).Assembly);
		return services;
	}
}
```