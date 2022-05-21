namespace Server.src;

public class HttpServer
{
    public HttpServer(RedisService redisService)
    {
        var builder = WebApplication.CreateBuilder();
        _app = builder.Build();

        _redisService = redisService;
    }

    public void Run()
    {
        this._app.Run(async (context) =>
        {
            var response = context.Response;
            var request = context.Request;
            var path = request.Path;

            if (path == "/getChat" && request.Method == "GET")
                GetChat(response);

            if (path == "/sendMessage" && request.Method == "POST")
                AddMessage(request);
        });
        this._app.Run();
    }

    private void GetChat(HttpResponse response)
    {
        var messages = _redisService.GetAll();
        response.WriteAsJsonAsync(messages);
    }

    private async void AddMessage(HttpRequest request)
    {
        try
        {
            string name = request.Query["name"];
            string text = request.Query["message"];

            await _redisService.Set(new Message() { Name = name, Text = text });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private WebApplication _app;
    private RedisService _redisService;
}