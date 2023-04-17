using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;

namespace Astramus.Worker;

public class WindowsBackgroundService : BackgroundService
{
    private readonly ILogger<WindowsBackgroundService> _logger;
    private readonly JokeService _jokeService;
    private  EventingBasicConsumer _consumer;
    private IModel _rabbitChannel;
    private IConnection _rabbitConnection;
    IOptions<MasterConf> _masterConf;

    public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger, JokeService jokeService, IOptions<MasterConf> masterConf)
    {
        _logger = logger;
        _jokeService = jokeService;
        _masterConf = masterConf;
    }



    private void HandleReceivedEvent(Object? model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        _logger.LogInformation(" [x] Received {message}", message);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Master Details {color} ,and max is {maxEntries}",
                _masterConf.Value.Color, _masterConf.Value.MaxEntries);
           // StartRabbitAndCreateConsummer();
            _logger.LogInformation("Rabbit MQ Starting");
            //_consumer.Received += HandleReceivedEvent;

            string joke = _jokeService.GetJoke();
            _logger.LogWarning("We started: -> {Joke}", joke);

            await Task.Delay(1000, stoppingToken);

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    string joke = _jokeService.GetJoke();
            //    _logger.LogWarning("{Joke}", joke);

            //     await Task.Delay(TimeSpan.FromSeconds(40), stoppingToken);
            //}
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Task cancelled");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }
    }

    private void StartRabbitAndCreateConsummer()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _rabbitConnection = factory.CreateConnection();
        _rabbitChannel = _rabbitConnection.CreateModel();

        _rabbitChannel.QueueDeclare(queue: "hello",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        _logger.LogInformation(" [*] Waiting for messages.");

         _consumer = new EventingBasicConsumer(_rabbitChannel);

        _rabbitChannel.BasicConsume(queue: "hello",
                             autoAck: true,
                             consumer: _consumer);

    }

    private void CloseRabbitRes()
    {
       /* _rabbitChannel.Close();
        _rabbitConnection.Close();*/
    }

    public override void Dispose()
    {
        _consumer.Received -= HandleReceivedEvent;
        CloseRabbitRes();
        _logger.LogInformation("Rabbit MQ Stopping");

        base.Dispose();
    }


}
