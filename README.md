# MqttNet.DependencyInjection
基于MqttNet进行的服务封装中间件

## Mqtt服务配置
### 添加服务配置

首先在 Startup.cs文件进行配置：

```cs
public void ConfigureServices(IServiceCollection services)
{
    //......
    services.AddMqttClient(options => {
        options.Server = "127.0.0.1"; //Mqtt服务器ip
        options.Port = 60005;  //端口
        options.UserName = "";  //用户名
        options.Passowrd = ""; //密码
        options.ClientId = "ClientId";  //配置ClientId
        // 以下为可选项（括号内为默认值）
        // options.KeepAlivePeriod = TimeSpan.FromSeconds(60);
        // options.Timeout = TimeSpan.FromSeconds(10);        //通讯超时
        // options.ReconnectInterval = TimeSpan.FromSeconds(5); //断线重连间隔
        // options.CleanSession = true;
        // options.ProtocolVersion = MqttProtocolVersion.V311;
        // options.UseTls = false;
    }).AddEventHandler<MqttClientEventHandler>()  //Topic配置和消息处理 handler类，
    //开启心跳
    .WithHeartBeat(options => 
    {
        options.EnableHeartBeat = true; //启用
        options.PubHeartBeatTopic = "Clould/HeartBeat"; //发送心跳的Topic
        options.HeartBeatinterval = TimeSpan.FromSeconds(1); //时间间隔
        options.DeviceNo = "deviceNo"; //设备Id
        options.CustmData = new{}; //自定义数据，心跳自动携带
    })
    //开启动态订阅和退订
    .WithDynamicScribe(options =>
    {
        options.EnableDynamicSubcribe = true; //启用
        options.SubcribeHeartBeatTopic = "Clould/HeartBeat"; //接收心跳的Topic
        options.DynamicSubcribeinterval = TimeSpan.FromSeconds(1); //时间间隔
    });
    
}
```

### 处理类编写

需要继承 AbsractClientHandler 抽象类

```cs
    public class MqttClientEventHandler : AbsractClientHandler
    {


        public MqttClientEventHandler(IMqttPublisher mqttPublisher) : base(mqttPublisher)
        {

        }
        //设置需要订阅的Topic列表，
        //未开启动态订阅，将遍历所有主题全部订阅
        //开启动态订阅，通过心跳解析，对其对应的ClientId下的Topic列表进行订阅和退订
        public override void SetTopic(out IEnumerable<ClientTopic> mqttClient)
        {
            mqttClient = new List<ClientTopic>()
            {
                new ClientTopic(){ 
                    DeviceNo = "deviceNo", 
                    TopicList = new List<string>
                    {
                        "topic1",
                        "topic2"
                    }
                }
            };
        }

        //心跳处理方法
        public override Task HeartBeatReceivedAsync(HeartBeatArgs args)
        {
            return Task.CompletedTask;
        }

        //消息处理方法（异常会被捕获并记录日志，不会中断连接）
        public override async Task MessageReceivedAsync(MessageReceiveArgs args)
        {
            await _mqttPublisher.PublishAsync("reply", "ok"); //内置的发送信息服务
            // 可指定 QoS / Retain：
            // await _mqttPublisher.PublishAsync("reply", "ok", MqttQualityOfServiceLevel.AtLeastOnce, retain: false);
            Console.WriteLine("### 收到来自服务器端的消息 ###");
            // 收到的消息主题
            string topic = args.Topic;
            // 收到的的消息内容
            string payload = args.Message;
            // 收到的发送级别(Qos)
            var qos = args.QosLevel;
            // 收到的消息保持形式
            bool retain = args.Retain;
            // 消息在处理方法返回后自动确认，无需手动调用 args.AcknowledgeAsync
            var message = $"主题: [{topic}] 内容: [{payload}] Qos: [{qos}] Retain:[{retain}]";
            Console.WriteLine(message);
        }


    }
```

### 发送消息

通过依赖注入获取发送信息接口服务

```cs
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMqttPublisher _mqttPublisher;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,IMqttPublisher mqttPublisher)
        {
            _logger = logger;
            _mqttPublisher = mqttPublisher; //依赖注入赋值对象
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _mqttPublisher.PublishAsync("topic","message"); //调用发送信息方法

            
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
     }
```
