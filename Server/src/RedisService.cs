using System;
using System.Text.Json;
using StackExchange.Redis;

namespace Server.src;

public class RedisService
{
    public RedisService()
    {
        _connectionMultiplexer = ConnectionMultiplexer.Connect("redis:6379,abortConnect=False");
        _database = _connectionMultiplexer.GetDatabase();

        _applicationContext = new ApplicationContext();
        _guids = new List<Guid>();
        this.Init();
    }

    private async void Init()
    {
        var messages = _applicationContext.Messages.ToList();

        foreach (var item in messages)
        {
            var json = JsonSerializer.Serialize(item);
            await _database.StringSetAsync(GetNewKey(), json);
        }
    }

    private string GetNewKey()
    {
        var guid = Guid.NewGuid();
        _guids.Add(guid);

        return guid.ToString();
    }

    public async Task Set(Message message)
    {
        var json = JsonSerializer.Serialize(message);
        await _database.StringSetAsync(GetNewKey(), json);

        await _applicationContext.Messages.AddRangeAsync(message);
        _applicationContext.SaveChanges();
    }

    public List<Message> GetAll()
    {
        List<Message> list = new List<Message>();
        foreach(var guid in _guids)
        { 
            Message? message = JsonSerializer.Deserialize<Message>(_database.StringGet(guid.ToString()));

            if(message is not null)
                list.Add(message);
        }
        return list;
    }

    private ConnectionMultiplexer _connectionMultiplexer;
    private IDatabase _database;
    private ApplicationContext _applicationContext;
    private List<Guid> _guids;
}
