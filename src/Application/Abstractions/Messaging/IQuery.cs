using Ardalis.Result;
using MediatR;

namespace Connecvita.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}