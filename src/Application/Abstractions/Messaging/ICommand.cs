using Ardalis.Result;
using MediatR;

namespace Connecvita.Application.Abstractions.Messaging;

/// <summary>
/// Command abstraction. This abstraction wraps the MediatR's IRequest.
/// </summary>
public interface ICommand : IRequest<Result>, IBaseCommand
{
}

/// <summary>
/// Command abstraction. This abstraction wraps the MediatR's IRequest.
/// </summary>
public interface ICommand<TReponse> : IRequest<Result<TReponse>>, IBaseCommand
{
}

public interface IBaseCommand
{
}