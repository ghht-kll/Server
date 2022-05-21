using Server.src;

RedisService redisService = new RedisService();

RabbitMQService rabbitMQService = new RabbitMQService(redisService);
rabbitMQService.ReceivingMessages();
rabbitMQService.ProcessingRPCRequests();

HttpServer httpServer = new HttpServer(redisService);
httpServer.Run();