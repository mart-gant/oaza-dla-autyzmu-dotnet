using MediatR;
using OazaDlaAutyzmu.Application.DTOs;

namespace OazaDlaAutyzmu.Application.Queries.Forum;

public record GetTopicByIdQuery : IRequest<ForumTopicDto?>
{
    public int Id { get; init; }
}
