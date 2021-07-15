package cn.wyt.changeBlock;
import com.github.jedis.lock.JedisLock;import com.rabbitmq.client.*;
import redis.clients.jedis.Jedis;

import java.io.IOException;
import java.util.concurrent.TimeoutException;
public class blockChanger {
    public static void main(String[] args) {
        Jedis jedis=new Jedis();
//        draw(jedis,5,6,28,6);
//        draw(jedis,5,28,5,42);
//        draw(jedis,6,16,17,19);
//        draw(jedis,18,12,18,22);
//        draw(jedis,15,34,18,42);
//        draw(jedis,32,19,32,36);
//        draw(jedis,39,8,46,11);
//        draw(jedis,0,0,49,0);
//        draw(jedis,0,0,0,49);
//        draw(jedis,49,0,49,49);


//        String nom="mapVisited";
//        String nom="mapView";
//        String nom="mapDetected";
        String[] names={"mapVisited","mapView","mapDetected"};

        for (int i = 0; i < names.length; i++) {
            String nom=names[i];
            System.out.println(names[i]);
            draw(jedis,0,0,49,49,false,nom);
            display(jedis,nom);
        }




    }
    public  static  void draw(Jedis db,int startx,int starty,int endx,int endy,boolean bit,String mapName){
        for (int i = startx; i < endx+1;i++) {
            for (int j = starty ; j < endy+1; j++) {
                int offset=50*i+j;
                db.setbit(mapName,i*50+j,bit);
            }
        }
    }
    private static void display(Jedis db,String mapName){
        for (int i = 0; i < 50;i++) {
            for (int j = 0 ; j < 50; j++) {
                int offset=50*i+j;
                System.out.print(db.getbit(mapName,offset)==true?1:0);
                System.out.print(" ");
            }
            System.out.println();
        }
        System.out.println("finished");
    }
}
