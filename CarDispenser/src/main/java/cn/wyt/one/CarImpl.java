package cn.wyt.one;

import java.io.IOException;
import java.util.concurrent.TimeoutException;

public interface CarImpl {
    void ScanAndRecord();
    void writeMapDetected(int x,int y);
    void writeMapVisited(int x,int y);
    void writeMapViewed(int x,int y);
    void flushAllSteps() throws IOException, TimeoutException;
    boolean isVisited(int x,int y);
    boolean isPathValid();
    boolean isPathSecure();
    void updateStep(String step);
    void updateLocation();
    char[] toBinaryChars(String original);
    void move();
    void terminate();
    String statusReport();
    void setStatusRunning();
    void setStatusRoutEmpty();
    void setStatusReloading();
}
