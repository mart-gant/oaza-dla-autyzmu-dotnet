using MediatR;
using OazaDlaAutyzmu.Application.DTOs;

namespace OazaDlaAutyzmu.Application.Queries.Forum;

public record GetPostsByTopicQuery : IRequest<List<ForumPostDto>>
{
    public int TopicId { get; init; }
}
