package cn.wyt.one;
import com.github.jedis.lock.JedisLock;import com.rabbitmq.client.*;
import redis.clients.jedis.Jedis;

import java.io.IOException;
import java.util.concurrent.TimeoutException;

public class Car implements Runnable,CarImpl {
    int runTimeWidth;
    int x;
    int y;
    Jedis db;
    JedisLock jedisLock;
    CommandDispenser commandDispenser;//从这里接收下一步步骤指南
    String carName;//Car1,Car2,......In redis
    String nextStep;//json字符串
    Step realNextStep;//真正的类型实例
    boolean keepRunning;

    public Car(int x, int y, String carName) {
        runTimeWidth=0;
        this.x = x;
        this.y = y;
        this.carName = carName;
        keepRunning=true;
        db = new Jedis();
//        jedisLock = new JedisLock(db, "lockname", 10000, 30000);
        realNextStep=new Step(x+"",y+"");
        try {
            commandDispenser=new CommandDispenser();
        } catch (IOException e) {
            e.printStackTrace();
        } catch (TimeoutException e) {
            e.printStackTrace();
        }
    }

    @Override
    public void run() {
        updateLocation();
        while (keepRunning){
            runTimeWidth++;
            if (db.get(carName+":Status").equalsIgnoreCase("2")||db.get(carName+":Status").equalsIgnoreCase("1")){
                try {
                    Thread.sleep(20);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                continue;
            }//正在装填就不执行了

            try {
                nextStep=commandDispenser.getCarStep(this.carName);
            } catch (IOException e) {
                e.printStackTrace();
            }
            if ( (nextStep==null||nextStep.isEmpty() )){//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!//
                String isRoutEmpty=db.llen(carName+":RoutList")+"";
                if ( isRoutEmpty.equals("0")){
                    this.setStatusRoutEmpty();
                    continue;
                }else {
                    ;
                }
            }else {
                this.updateStep(nextStep);
            }//一次读取一条路径任务，如果有的话更新，没有的,检查是不是都读空了，是的话状态设为1（清空）


            ScanAndRecord();//扫描写入redis黑板

            if (!isPathSecure()){
                this.realNextStep.setDestX(this.x+"");
                this.realNextStep.setDestY(this.y+"");
                try {
                    flushAllSteps();////////////////
                    this.setStatusRoutEmpty();
                } catch (IOException e) {
                    e.printStackTrace();
                } catch (TimeoutException e) {
                    e.printStackTrace();
                }
            }else {;}//有障碍则清理所有的步骤

            if (!isPathValid()){
                try {
                    flushAllSteps();
                    this.setStatusRoutEmpty();
                } catch (IOException e) {
                    e.printStackTrace();
                } catch (TimeoutException e) {
                    e.printStackTrace();
                }
            }else {;}//路径无意义，清除所有的步骤

            move();//完全安全，则马上移动（本地）

            this.updateLocation();//redis写入当前车辆位置

//            jedisLock.release();//解锁



            try {
                Thread.sleep(120);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }//等待某某毫秒


        }
    }


    @Override
    public void ScanAndRecord() {
        writeMapVisited(x,y);
        int xmin=x-5>=0?x-5:0;
        int xmax=x+6<=49?x+6:50;
        int ymin=y-5>=0?y-5:0;
        int ymax=y+6<=49?y+6:50;
        for (int i = xmin; i < xmax; i++) {
            for(int j = ymin; j < ymax; j++) {
                if ((x-i)*(x-i)+(y-j)*(y-j)<=25){
                    writeMapViewed(x,y);
                    if (db.getbit("mapBlock",50*i+j)==true){
                        writeMapDetected(i,j);
                    }else
                    {;}
                }else {;}
            }
        }
    }

    @Override
    public void writeMapDetected(int x, int y) {
        db.setbit("mapDetected",x*50+y,true);
    }
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    @Override
    public void writeMapVisited(int x, int y) {
        db.setbit("mapVisited",x*50+y,true);
    }

    @Override
    public void writeMapViewed(int x, int y) {
        db.setbit("mapView",x*50+y,true);
    }

    @Override
    public void flushAllSteps() throws IOException, TimeoutException {//问题在这里!!!!!!!!!!!!!!
        flushMQ();
        db.del(this.carName+":RoutList");
    }//清空所有的步序指导
    public void flushMQ() throws IOException, TimeoutException {
        new MQPurger().flushMQ(this.carName);
    }

    @Override
    public boolean isVisited(int x, int y) {
        return false;
    }//弃用

    @Override
    public boolean isPathValid() {
        String jsonRaw=db.lpop(this.carName+":RoutList");
        if (null==jsonRaw||jsonRaw.isEmpty()){
            return  true;
        }
        db.lpush(this.carName+":RoutList",jsonRaw);
//        String jsonRaw=db.brpoplpush()
        Step stepToExam=Step.Deserialize(jsonRaw);
        if (db.getbit("mapView",stepToExam.getDestXInt()*50+stepToExam.getDestYInt())){//如果最后一步已经探明
            return  false;
        }else {
            return true;
        }
    }//看路径有没有走的必要

    @Override
    public boolean isPathSecure() {
       return(!db.getbit("mapDetected",realNextStep.getDestXInt()*50+realNextStep.getDestYInt()));
    }//看下一步是不是踩在障碍物上

    @Override
    public void updateStep(String step) {
        this.nextStep=step;
        this.realNextStep=Step.Deserialize(nextStep);
    }

    @Override
    public void updateLocation() {
        db.hset(this.carName+":Position","x",String.valueOf(this.x));
        db.hset(this.carName+":Position","y",String.valueOf(this.y));
    }

    @Override
    public char[] toBinaryChars(String original) {
        char [] fresh=original.toCharArray();
        StringBuffer sb=new StringBuffer();
        for (int i=0;i<fresh.length;i++){
            sb.append(String.format("%8s", Integer.toBinaryString(fresh[i])).replace(' ', '0'));
        }
        return  sb.toString().toCharArray();
    }

    @Override
    public void move() {
        this.x=realNextStep.getDestXInt();
        this.y=realNextStep.getDestYInt();
    }

    @Override
    public void terminate() {
        this.keepRunning=false;
    }

    @Override
    public String statusReport() {
        return "x:"+this.x+"y:"+this.y+" Name="+this.carName+" RunTimeWidth:"+this.runTimeWidth;
    }

    @Override
    public void setStatusRunning() {
        db.set(carName+":Status","0");
    }

    @Override
    public void setStatusRoutEmpty() {
        db.set(carName+":Status","1");
    }

    @Override
    public void setStatusReloading() {
        db.set(carName+":Status","2");
    }


}
