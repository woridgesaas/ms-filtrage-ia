
namespace ms_filtering_ia.Messaging.Publisher;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ms_filtering_ia.Messaging.Events;
using ms_filtering_ia.Options;
using ms_filtering_ia.Services;
using RabbitMQ.Client;

public sealed class RabbitMQPublisher : IRabbitMQPublisher
{
    private readonly IRabbitMQConnection  _connection;
    private readonly RabbitMQSettings     _settings;
    private readonly ILogger<RabbitMQPublisher> _logger;

    public RabbitMQPublisher(
        IRabbitMQConnection connection,
        IOptions<RabbitMQSettings> settings,
        ILogger<RabbitMQPublisher> logger)
    {
        _connection = connection;
        _settings   = settings.Value;
        _logger     = logger;
    }

    public async Task PublishCandidateAcceptedAsync(CandidateAcceptedEvent @event)
    {
        await using var channel = await _connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange:   _settings.Exchange,
            type:       ExchangeType.Topic,
            durable:    true,
            autoDelete: false);

        await channel.QueueDeclareAsync(
            queue:      _settings.Queues.CandidateAccepted,
            durable:    true,
            exclusive:  false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue:      _settings.Queues.CandidateAccepted,
            exchange:   _settings.Exchange,
            routingKey: _settings.RoutingKeys.CandidateAccepted);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(@event));

        var props = new BasicProperties { Persistent = true };

        await channel.BasicPublishAsync(
            exchange:   _settings.Exchange,
            routingKey: _settings.RoutingKeys.CandidateAccepted,
            mandatory:  false,
            basicProperties: props,
            body:       body);

        _logger.LogInformation(
            "[RabbitMQPublisher] CandidateAccepted publié — {Name} | Score={Score}",
            @event.CandidateName, @event.AiScore);
    }
}
