using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace CentralController
{
    class Program
    {
        static ConnectionFactory factory;
        static IConnection connection;
        static IModel channel;
        static IDatabase db;
        static void Main(string[] args)
        {
            factory = new ConnectionFactory();
            factory.UserName = "guest";
            factory.Password = "guest";
            string host = "localhost:6379";
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            //MQ初始化


            //取连接对象
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(host);
            //取得DB对象
            db = redis.GetDatabase();
            bool keepRunning = true;
            while (keepRunning)
            {
                int carMax;
                int.TryParse(db.StringGet("carMax"), out carMax);
                //职责1:转移路径指令
                for (int i = 1; i < carMax+1; i++)
                {
                    string carName = "Car" + i;
                    string jsonToSend = stepAcquire(carName);
                    if (GetMessageCount(carName)>=5)
                    {
                        Console.WriteLine(carName+" MQ上超过5个待处理任务");
                        //Console.WriteLine("");
                        Console.WriteLine("--------------------------------------------");
                        continue;
                    }
                    else
                    {
                        ;
                    }//队列里至多放五个
                    if (jsonToSend.Equals("dontgivemq"))
                    {
                        Console.WriteLine(carName+" 的redis任务队列为空,无法向MQ添加");
                        Console.WriteLine("--------------------------------------------");
                        continue;
                    }
                    else
                    {
                        ;
                    }//如果没拿到，也不放
                    //Console.WriteLine("拿到了指令给"+carName);
                    Console.WriteLine("向 "+carName+" 转移了路径指令");
                    Console.WriteLine("--------------------------------------------");
                    publishToMq("exgCarSteps", carName, jsonToSend);//放入


                }//把REDIS黑板里面的任务序列传到car监听的队列上面的交换机上

                //职责2:命令产生路径
                for (int i = 1; i < carMax+1; i++)
                {
                    Console.WriteLine("检测 Car" +i+
                        " 是否需要重新补充任务");
                    if (isStepsPurged("Car" + i))
                    {
                        Console.BackgroundColor = ConsoleColor.Blue; //设置背景色
                        Console.ForegroundColor = ConsoleColor.White; //设置前景色，即字体颜色
                        Console.WriteLine("Car"+i+" 已经命令导航器补充");
                        Console.ResetColor(); //将控制台的前景色和背景色设为默认值
                        orderANewRoute("Car" + i);

                    }
                    else
                    {
                        Console.WriteLine("检测 Car" + i + "的 redis mq 任务未完全清空");
                    }
                }//如果路径被清空了，马上通知导航器造路
                Console.WriteLine("--------------------------------------------");

                System.Threading.Thread.Sleep(400);//等待80ms


            }
        }
        public static void publishToMq(string exgName,string bindKey,string info)
        {
                byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(info);
                channel.BasicPublish(exgName, bindKey, null, messageBodyBytes);
        }
        public static int GetMessageCount(string queueName)
        {
            int res;
            int.TryParse(channel.MessageCount(queueName)+"", out res);
            return res;
        }
        public static string stepAcquire(string carName)
        {
            string jsonRaw = db.ListRightPop(carName + ":RoutList");
            if (string.IsNullOrEmpty(jsonRaw)||string.IsNullOrWhiteSpace(jsonRaw))
            {
                return "dontgivemq";
            }
            else
            {
                return jsonRaw;
            }
        }
        public static bool isStepsPurged(string carName)
        {
            string rds = db.ListGetByIndex(carName+":RoutList", 0);
            bool one = string.IsNullOrEmpty(rds);//黑板被清空，则真
            bool two = GetMessageCount(carName)==0?true:false;//队列被清空，则真
            //必须全true才可以返回true;
            return one & two;

        }
        public static void orderANewRoute(string carName)
        {
            publishToMq("exgNavigator", "Navigator", carName);
        }
    }
}
