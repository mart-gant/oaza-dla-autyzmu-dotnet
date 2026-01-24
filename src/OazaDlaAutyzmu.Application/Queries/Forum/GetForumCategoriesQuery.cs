using MediatR;
using OazaDlaAutyzmu.Application.DTOs;

namespace OazaDlaAutyzmu.Application.Queries.Forum;

public record GetForumCategoriesQuery : IRequest<List<ForumCategoryDto>>;
