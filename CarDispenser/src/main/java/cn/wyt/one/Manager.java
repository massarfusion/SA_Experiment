package cn.wyt.one;
import redis.clients.jedis.Jedis;

import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Scanner;
import java.util.concurrent.TimeoutException;

public class Manager {
    static boolean displayInfo=false;//要不要打印信息
    static boolean terminateAllCars=false;//
    static boolean shutDownAll=false;
    public static void main(String[] args) throws IOException, TimeoutException {
        Scanner in=new Scanner(System.in);

        HashMap<String,Car> cars=new HashMap<String, Car>();
        CommandDispenser commandDispenser=new CommandDispenser();
        Jedis jedis=new Jedis();
        boolean running=true;
        int carMax=1;//每次重置，每次一辆车起步
        jedis.set("carMax",carMax+"");
        for (int i = 1; i <= carMax; i++) {
            cars.put("Car"+i,new Car(3,3,"Car"+i));
            jedis.del("Car"+i+":Position");
            jedis.del("Car"+i+":RoutList");//及时清理脏数据
        }
        for (Car car:cars.values()
             ) {
            car.setStatusRoutEmpty();//一开始状态设定为空“1”（实际上也已经清空），否则不能继续执行
            new Thread(car)
                    .start();
        }

        while (running){
            String order=commandDispenser.getCarStep("Manager");
            if (order==null||order.isEmpty()){
                ;
            }else {
                carMax++;
                commandDispenser.addNewCarQueue("Car"+carMax);
                jedis.set("carMax",carMax+"");
                Car carTemp=new Car(3,3,"Car"+carMax);
                carTemp.setStatusRoutEmpty();//一开始状态设定为空“1”（实际上也已经清空），否则不能继续执行
                jedis.del("Car"+carMax+":Position");
                jedis.del("Car"+carMax+":RoutList");
                cars.put("Car"+carMax,carTemp);
                new Thread(carTemp).start();//建立新车
            }

            System.out.println("输入'stop'(不加引号)终止所有car,输入exit结束本管理器,输入其他的可以看车辆信息(需要加车请手动清理本命令阻塞)");
            String input=in.nextLine();
            if (input.trim().equalsIgnoreCase("stop")){
                for (Car car:cars.values()
                     ) {
                    car.terminate();
                }
                System.out.println("All cars Terminated");
            }
            else if (input.trim().equalsIgnoreCase("exit")){
                for (Car car:cars.values()
                ) {
                    car.terminate();
                }
                commandDispenser.channel.close();
                commandDispenser.connection.close();
                System.out.println("All shut down");
//                running=false;
                System.exit(0);
            }
            else {
                for (Car car:cars.values()
                ) {
                    System.out.println(car.statusReport());
                }
            }
        }

    }

}
