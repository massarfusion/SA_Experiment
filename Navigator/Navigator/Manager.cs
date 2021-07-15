using AStar;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Navigator
{
    class Manager
    {
        static void Main(string[] args)
        {
            string host = "localhost:6379";
            //取连接对象
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(host);
            //取得DB对象
            IDatabase db = redis.GetDatabase();
            

            Calc calc = new Calc();
            Console.WriteLine("Start runnning");
            Random rand = new Random();
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        //redis.Close();
                        Console.WriteLine("下面开始连接测试");
                        bool ServerEcho = redis.IsConnected;
                        Console.WriteLine("ServerEcho:"+ServerEcho);
                        if (!ServerEcho)
                        {
                            redis = ConnectionMultiplexer.Connect(host);
                            db = redis.GetDatabase();
                        }//连接重连尝试

                        var body = ea.Body.ToArray();
                        string message = Encoding.UTF8.GetString(body);
                        Request rq = new Request(message);
                        Console.WriteLine("尝试为Car:"+rq.carName+"规划路径中");

                        
                        
                        for (int i = 1; i < 100; i++)
                        {
                            //如果正在装填，直接略过，如果不是的话就装填
                            if (db.StringGet(rq.carName + ":Status").Equals("2")|| db.StringGet(rq.carName + ":Status").Equals("0"))
                            {
                                Console.WriteLine(rq.carName+"不需要装填,状态是" + db.StringGet(rq.carName + ":Status") +
                                    ",本导航器暂不规划");
                                break;
                            }
                            else
                            {
                                db.StringSet(rq.carName + ":Status","2");
                            }

                            //随机选择一个没探索的点去规划路径
                            int destiOffset = rand.Next(0, 2499);
                            int destiX = destiOffset / 50;
                            int destiY = destiOffset % 50;
                            bool mark= db.StringGetBit("mapView", destiOffset);
                            if (mark)
                            {
                                ;
                            }
                            else
                            {
                                int a2;
                                int.TryParse(db.HashGet(rq.carName + ":Position", "x"), out a2);
                                int carX = a2;
                                int a1;
                                int.TryParse(db.HashGet(rq.carName + ":Position", "y"), out a1);
                                int carY = a1;

                                Console.WriteLine("起始X " +carX+
                                    " 起始Y: " +carY+
                                    " 终点X: " +destiX+
                                    " 终点Y: " +destiY+
                                    "");
                                //以下为路径规划主程序
                                int[,] tst = new int[50, 50];
                                for (int k = 0; k < 50; k++)
                                {
                                    for (int j = 0; j < 50; j++)
                                    {
                                        tst[k, j] = db.StringGetBit("mapDetected", k * 50 + j) ? -1 : 0;
                                    }
                                }
                                tst[carX, carY] = 1;
                                tst[destiX, destiY] = 2;
                                ArrayList al = calc.finalResult(tst, carX, carY, destiX, destiY);//寻路测试完成

                                Console.WriteLine("寻路已经完成  向 "+rq.carName+" 的黑板任务队列追加任务");
                                //以下为路径发送
                                foreach (AStarNode2D node in al)
                                {
                                    Step tmpStep = new Step(node.X.ToString(), node.Y.ToString());
                                    string rawJson = Step.Serial(tmpStep);
                                    db.ListLeftPush(rq.carName + ":RoutList", rawJson);
                                }

                                

                                break;
                            }
                        }
                        //车辆装填设为0“正常运行”
                        db.StringSet(rq.carName + ":Status", "0");
                        Thread.Sleep(20);
                        Console.WriteLine("-----------------------------------------------");



                    };
                    channel.BasicConsume(queue: "Navigator",
                                         autoAck: true,
                                         consumer: consumer);

                    Console.WriteLine("--------输入任何值再回车以便退出本程序---------");
                    Console.ReadLine();
                    
                }
            }



        }
    }
}
