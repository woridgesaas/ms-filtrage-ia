namespace ms_filtering_ia.Messaging.Publisher;

using ms_filtering_ia.Messaging.Events;

public interface IRabbitMQPublisher
{
    Task PublishCandidateAcceptedAsync(CandidateAcceptedEvent @event);
}