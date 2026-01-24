using MediatR;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Commands.Forum;

public class ToggleTopicLockCommandHandler : IRequestHandler<ToggleTopicLockCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public ToggleTopicLockCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ToggleTopicLockCommand request, CancellationToken cancellationToken)
    {
        var topic = await _context.ForumTopics.FindAsync(request.TopicId);
        if (topic == null)
            return false;

        topic.IsLocked = !topic.IsLocked;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
