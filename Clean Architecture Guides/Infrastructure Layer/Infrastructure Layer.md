One of the two most outer layers of the architecture. It is responsible for interfacing with any external concerns. This layer implements all abstractions found in the `Application` layer.

![[Infrastructure Layer overview.png]]

#### Project Setup

Like the application layer, we need to add a `DependencyInjection` class to hold any DI setup for the project. Create this class at the root of the project.

```csharp
public static class DependencyInject
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{

		return services;
	}
}
```

> [!Note]
> We include the `IConfiguration` interface here so that we can get access to the connection string information. To get the `IConfiguration` interface we need to add the `Microsoft.Extensions.Configuration.Abstractions` nuget package to this project.


### Clock Service

We are going to add the `DateTimeProvider` class which implements the `IDateTimeProvider` interface.

1. Add a folder in the root of the project called `Clock`. Place the `DateTimeProvider` class inside this folder. The implementation should look like this:

```csharp
internal sealed class DateTimeProvider : IDateTimeProvider
{
	public DateTime UtcNow => DateTime.UtcNow;
}


// Register this service in the Dependency Injection class

public static class DependencyInject
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddTransient<IDateTimeProvider, DateTimeProvider>();
		
		return services;
	}
}
```

#### Adding EF Core to the Project

1. Add the `EF Core` nuget package along with the appropriate provider (SQL Server, PostgreSQL etc.).
2. Add the `DBContext` class at the root of the project. In this case we will call it `ApplicationDbContext`. If you have more than one context, it may be useful to place them into a folder called `Contexts`. The `ApplicationDbContext` class will look like this:

```csharp
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{

	public ApplicationDbContext(DbContextOptions options) : base(options)
	{
	}
}
```

3. Register EF Core in the Dependency Injection class
```csharp
public static class DependencyInject
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		//...

		var connectionString = configuration.GetConnectionString("Database") ??
			throw new ArgumentNullException(nameof(configuration));

		services.AddDbContext<ApplicationDbContext>(options =>
		{
			options.UseSqlServer(connectionString);
		})
		
		return services;
	}
}

```

#### Adding the Domain Entities to the DbContext

Since we have implemented our domain entities in a persistent ignorant manner, we will use EF Cores fluent configurations to map our entities to the database.

1. Create a `Configurations` folder and add our first configuration class called `ApartmentConfiguration`. The configuration class will look like this:

```csharp
interal sealed class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
	public void Configure(EntityTypeBuilder<Apartment> builder)
	{
		builder.ToTable("apartments");

		builder.HasKey(apartment => apartment.Id);

		builder.OwnsOne(apartment => apartment.Address);

		builder.Property(apartment => apartment.Name)
			.HasMaxLength(200)
			.HasConversion(name => name.Value, value => new Description(value));

		builder.Property(apartment => apartment.Description)
			.HasMaxLength(2000)
			.HasConversion(description => description.Value, value => new Description(value));

		builder.OwnsOne(apartment => apartment.Price, priceBuilder => 
		{
			priceBuilder.Property(money => money.Currency)
				.HasConversion(currency => currency.Code, code => Currency.FromCode(code));
		});

		builder.OwnsOne(apartment => apartment.CleaningFee, priceBuilder => {
			priceBuilder.Property(money => money.Currency)
			.HasConversion(currency => currency.Code, code => Currency.FromCode(code));
		});
	}
}
```

The other configurations are very similar. To learn more about EF Cores fluent api see [Creating and Configuring a Model - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/modeling/)

2. Automatically applying entity configurations by overriding the `OnModelCreating` method in the `ApplicationDbContext` class as seen below:

```csharp
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{

	public ApplicationDbContext(DbContextOptions options) : base(options)
	{
	}


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		
	modelBuilder.ApplyConfigurationsFromAssembly(typeOf(ApplicationDbContext).Assembly);
	
		base.OnModelCreating(modelBuilder);

	}
}

```

#### Implementing the Repositories

We will implement all the repository interfaces found in the `Application` layer in this project.
Do the following.

1.  Add a `Repositories` folder at the root of the project and add a generic Repository class as seen below:

```csharp
internal abstract class Repository<T> where T : Entity
{
	protected readonly ApplicationDbContext DbContext;

	protected Repository(ApplicationDbContext dbContext)
	{
		DbContext = dbContext;
	}

	public async Task<T?> GetByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default	
	)
	{

		return await DbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id,    cancellationToken);
	}

	public void Add(T entity)
	{
		DbContext.Add(entity);
	}
}

```

2. Now we will implement the `UserRepository` as an example. This class will implement the abstract `Respository<T>` class as well as the `IUserRepository` interface from the `Application` layer.

```csharp
internal sealed class UserRepository : Repository<User>, IUserRepository
{
	public UserRepository(ApplicationDbContext dbContext):base(dbContext)
	{
	}

}
```

3. You will implement the remainder of the repositories in the same manner. Next register the repositories in the `DependencyInjection` class.

```csharp
public static class DependencyInject
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		//...

		services.AddScoped<IUserRepository, UserRepository>();

		// remainder of the repositories here

// This will register the ApplicaitonDbContext as the implementation for the IUnitOfWork interface.
		services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
		
		return services;
	}
}

```


#### Adding the SQL Connection Factory

1. Add a new folder called `Data`. 
2. Add a new `SqlConnectionFactory` which implements the `ISqlConnectionFactory` from the `Application` layer.

```csharp

internal sealed class SqlConnectionFactory : ISqlConnectionFactory
{
	private readonly string _connectionString;

	public SqlConnectionFactory(string connectionString)
	{
		_connectionString = connectionString;
	}

	public IDbConnection CreateConnection()
	{
		var connection = new SqlConnection(_connectionString)		;
		connection.Open();

		return connection;
		
	}
}
```



#### Adding a `DateOnly` type handler for Dapper

If you are using a `DateOnly` type in your project, you will need to create a handler in order to use them with `Dapper`. This can be a template for other mapping types if needed:

```csharp

internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{

	public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
}

	public override void SetValue(IDbDataParameter parameter, DateOnly value)
	{
		parameter.DbType = DbType.Date;
		parameter.Value = value;
	}
```

We will now add both dependencies to the `DependencyInjection` class.

```csharp
public static class DependencyInject
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("Database") ??
			throw new ArgumentNullException(nameof(configuration));
			
		//...

		services.AddSingleton<ISqlConnectionFactory>(_ => 
		new SqlConnectionFactory(connectionString));

		SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
		
		return services;
	}
}

```

#### Publish Domain Events Using The Unit of Work Pattern

1. Override the `SaveChangesAsync` method in your DbContext class.

```csharp
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
	// This comes from the MediatR package
	private readonly IPublisher _publisher;

	public ApplicationDbContext(DbContextOptions options, IPublisher publisher) : base(options)
	{
		_publisher = publisher;_
	}


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		
	modelBuilder.ApplyConfigurationsFromAssembly(typeOf(ApplicationDbContext).Assembly);
	
		base.OnModelCreating(modelBuilder);

	}
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
	var result = await base.SaveChangesAsync(cancellationToken);

	await PublishDomainEventsAsync();

	return result;

}

	private async Task PublishDomainEventsAsync()
	{
		var domainEvents = ChangeTracker
			.Entities<Entity>()
			.Select(entry => entry.Entity)
			.SelectMany(entity => 
			{
				var domainEvents = entity.GetDomainEvents();

				eintity.ClearDomainEvents();

				return domainEvents;
			}).ToList():

		foreach(var domainEvent in domainEvents)
		{
			await _publisher.Publish(domainEvent);
		}
	}
}
```


#### Solving Race Conditions

1. Go back to the `Application` project and add a `ConcurrencyException` exception that looks like this:

```csharp
	public sealed class ConcurrencyException : Exception
	{
		public ConcurrencyException(string message, Exception innerException)
		: base(message, innerException)
		{			
		}
	}
```

2. Go back to the `ApplicationDbContext` class in the `Infrastructure` project and update the `SaveChangesAsync` method to look like this:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{

	try
	{
		var result = await base.SaveChangesAsync(cancellationToken);

		await PublishDomainEventsAsync();

		return result;
	}
	catch (DbUpdateConcurrencyException ex)
	{
		throw new ConcurrencyException("Concurrency exception occurred.", ex);
	}
	

}

```
One way to solve race conditions when saving information to the database is with optimistic concurrency.

3. Now you can go back to the `ReserveBookingCommandHandler` and update it to look like this:

```csharp
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

	try
	{
	var booking = Booking.Reserve(
			apartment,
			user.Id,
			duration,
			utcNow: DateTime.UtcNow,
			_pricingService);

		_bookingRepository.Add(booking);
		
	// this introduces a race condition which will be solved later
		await _unitOfWork.SaveChangesAsync(cancellationToken);
	

		return booking.Id;
	}
	catch (ConcurrencyException)
	{
		return Result.Failure<Guid>("There was an overlap error in the booking");
	}
		
}
```

There's one thing missing to help set up this optimistic concurrency. We need a column in the database to accomplish this. Since the Apartment entity has the LastBookedOnUtc column, this is a good entity to add a row version column on.

To define a row version column:
1. Go back to the `ApartmentConfiguration` class and add the following:

```csharp
builder.Property<uint>("version").IsRowVersion();
```

This will create a shadow property on the table.

Here is information from Microsoft on [Concurrency Conflicts.](https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations)


