package cn.wyt.one;

import com.rabbitmq.client.*;

import java.io.IOException;
import java.util.concurrent.TimeoutException;

public class CommandDispenser {
    ConnectionFactory connectionFactory;
    Connection connection;
    Channel channel;

    public CommandDispenser() throws IOException, TimeoutException {
        connectionFactory=new ConnectionFactory();
        connectionFactory.setHost("localhost");
        connectionFactory.setPort(5672);
        connectionFactory.setUsername("guest");
        connectionFactory.setPassword("guest");
        //建立连接
        connection=connectionFactory.newConnection();
        //双向信道
        channel=connection.createChannel();
    }

    public String getCarStep(String carName) throws IOException {
        GetResponse response = channel.basicGet(carName,true);
        if (response==null){return "";}else {;}
        byte[] resBody=response.getBody();
        return  new String(resBody);
    }
    public  void addNewCarQueue(String CarMark) throws IOException {
        //队列声明
        channel.queueDeclare(CarMark,true,false,false,null);
        //绑定，声明“暗号”
        channel.queueBind(CarMark,"exgCarSteps",CarMark);
    }
}
