using MediatR;
using OazaDlaAutyzmu.Application.DTOs;

namespace OazaDlaAutyzmu.Application.Queries.Forum;

public record GetTopicsByCategoryQuery : IRequest<List<ForumTopicDto>>
{
    public int CategoryId { get; init; }
}
