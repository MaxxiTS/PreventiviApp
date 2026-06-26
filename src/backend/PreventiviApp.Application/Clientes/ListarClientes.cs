using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;

namespace PreventiviApp.Application.Clientes;

public sealed record ListarClientesQuery : IRequest<IReadOnlyList<ClienteDto>>;

internal sealed class ListarClientesHandler(IClienteRepository repositorio)
    : IRequestHandler<ListarClientesQuery, IReadOnlyList<ClienteDto>>
{
    public async Task<IReadOnlyList<ClienteDto>> Handle(ListarClientesQuery request, CancellationToken cancellationToken)
    {
        var clientes = await repositorio.ListarAsync(cancellationToken);
        return clientes.Select(ClienteDto.De).ToList();
    }
}
