using RabbitMQRpcProduser;
using System.Text.Json;
using System.Diagnostics;

var rpcClient = new RpcClient();

Stopwatch stopwatch = new Stopwatch();

stopwatch.Start();
var response = rpcClient.Call("getChat");
stopwatch.Stop();

var list = JsonSerializer.Deserialize<List<Message>>(response);

foreach (var item in list)
    Console.WriteLine($"Name: {item.Name}, text: {item.Text}");

Console.WriteLine($"time: {stopwatch.ElapsedMilliseconds}");

rpcClient.Close();