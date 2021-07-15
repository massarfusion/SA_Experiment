package cn.wyt.one;
import com.alibaba.fastjson.JSON;



public class Step {
    private String destX;
    private String destY;

    public Step(String destX, String destY) {
        this.destX = destX;
        this.destY = destY;
    }
    public static String Seriailize(Step step){
        return JSON.toJSONString(step);
    }
    public static  Step Deserialize(String rawJson){
        Step sp=JSON.parseObject(rawJson, Step.class);
        return sp;
    }

    public String getDestX() {
        return destX;
    }

    public void setDestX(String destX) {
        this.destX = destX;
    }

    public String getDestY() {
        return destY;
    }

    public void setDestY(String destY) {
        this.destY = destY;
    }
    public  int getDestXInt(){
        return Integer.valueOf(destX).intValue();
    }
    public  int getDestYInt(){
        return Integer.valueOf(destY).intValue();
    }
}
