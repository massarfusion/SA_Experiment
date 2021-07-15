package cn.wyt.one;

import com.rabbitmq.client.Channel;
import com.rabbitmq.client.Connection;
import com.rabbitmq.client.ConnectionFactory;

import java.io.IOException;
import java.util.concurrent.TimeoutException;

public class MQPurger {
    ConnectionFactory connectionFactory;
    Connection connection;
    Channel channel;
    public MQPurger() throws IOException, TimeoutException {
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

    public void flushMQ(String carName) throws IOException, TimeoutException {

        channel.queuePurge(carName);
        channel.close();
        connection.close();
    }
}
