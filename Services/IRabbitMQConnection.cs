using RabbitMQ.Client;

namespace ms_filtering_ia.Services;

public interface IRabbitMQConnection
{
    Task<IChannel> CreateChannelAsync();
}