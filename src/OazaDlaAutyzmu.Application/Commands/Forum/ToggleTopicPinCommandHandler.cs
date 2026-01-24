using MediatR;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Commands.Forum;

public class ToggleTopicPinCommandHandler : IRequestHandler<ToggleTopicPinCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public ToggleTopicPinCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ToggleTopicPinCommand request, CancellationToken cancellationToken)
    {
        var topic = await _context.ForumTopics.FindAsync(request.TopicId);
        if (topic == null)
            return false;

        topic.IsPinned = !topic.IsPinned;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
