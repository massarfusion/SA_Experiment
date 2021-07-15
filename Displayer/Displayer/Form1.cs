using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using StackExchange.Redis;

namespace Displayer
{
    public partial class Form1 : Form
    {
        Graphics graphic ; //这句是创建画布g，根据窗体得到窗体的画布
        Graphics gra;
        Pen pen;
        Brush brushBlack;
        Brush brushRed;
        Brush brushWhite;
        Rectangle rectangleBack;
        IDatabase db;
        Bitmap bmp4World;
        Thread th;
        ArrayList carList;
        static ArrayList bitmapList;
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;//设置本窗体
            SetStyle(ControlStyles.UserPaint |
                  ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.ResizeRedraw |
                  ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            //UpdateStyles();
            graphic = this.displayArea.CreateGraphics();
            pen = new Pen(Color.Black,2);
            brushBlack = new SolidBrush(Color.Black);
            brushRed = new SolidBrush(Color.Red);
            brushWhite = new SolidBrush(Color.White);
            rectangleBack = new Rectangle(0,0,500,550);
            
            string host = "localhost:6379";
            //取连接对象
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(host);
            //取得DB对象
            db = redis.GetDatabase();

            carList = new ArrayList();//

            this.displayArea.BackColor = Color.Gray;//灰色背景

            bitmapList = new ArrayList();
            Thread threadAddBitmap = new Thread(GenerateBitmaps);
            threadAddBitmap.IsBackground = true;
            threadAddBitmap.Start();

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            th = new Thread(printBlackBoard);
            th.IsBackground = true;
            th.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            // Confirm user wants to close
            switch (MessageBox.Show(this, "Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            carList.Clear();
            //循环更新界面20次，每次睡眠200毫秒，
            for (int i = 0; i < 1; i++)
            {
                bool[,] visited = new bool[50, 50];
                int maxCarCount;
                int.TryParse(db.StringGet("carMax"), out maxCarCount);
                carList.Clear();
                for (int carIndex = 1; carIndex < maxCarCount + 1; carIndex++)
                {
                    string psX = db.HashGet("Car" + carIndex + ":Position", "x");
                    string psY = db.HashGet("Car" + carIndex + ":Position", "y");
                    carList.Add(new Car(psX, psY));
                }
                //准备好所有的基本绘图信息
                for (int visitedX = 0; visitedX < 50; visitedX+=1)
                {
                    for (int visitedY = 0; visitedY < 50; visitedY+=1)
                    {
                        if (db.StringGetBit("mapView", visitedX * 50 + visitedY))
                        {
                            graphic.FillEllipse(brushWhite, visitedY * 10 - 50, visitedX * 10 - 50, 100, 100);//刮奖
                            //graphic.DrawLine(pen, visitedY * 10, visitedX * 10, visitedY * 10 + 1, visitedX * 10);//路径点
                        }
                    }
                }//刮奖
                for (int visitedX = 0; visitedX < 50; visitedX++)
                {
                    for (int visitedY = 0; visitedY < 50; visitedY++)
                    {
                        if (db.StringGetBit("mapVisited", visitedX * 50 + visitedY))
                        {
                            //graphic.FillEllipse(brushWhite, visitedY * 10 - 50, visitedX * 10 - 50, 100, 100);//刮奖
                            graphic.DrawLine(pen, visitedY * 10, visitedX * 10, visitedY * 10 + 1, visitedX * 10);//路径点
                        }
                        if (db.StringGetBit("mapDetected", visitedX * 50 + visitedY))
                        {
                            graphic.FillRectangle(brushBlack, visitedY * 10, visitedX * 10, 10, 10);
                        }
                    }
                }//轨迹+路障
                //for (int visitedX = 0; visitedX < 50; visitedX++)
                //{
                //    for (int visitedY = 0; visitedY < 50; visitedY++)
                //    {
                //        if (db.StringGetBit("mapDetected", visitedX * 50 + visitedY))
                //        {
                //            graphic.FillRectangle(brushBlack, visitedY * 10, visitedX * 10, 10, 10);
                //        }
                //    }
                //}//路障
                //刮奖区上面刮开+画障碍+画路径点↑
                foreach (Car car in carList)
                {
                    int carX = car.posiXInt();
                    int carY = car.posiYInt();
                    graphic.FillRectangle(brushRed, carY * 10, carX * 10, 5, 5);
                }
            }
        }

        //void wipFaterDraw()
        //{
        //    Bitmap bmp4World = makePics();
        //    this.displayArea.Image = bmp4World;
        //}

        //private Bitmap makePics()
        //{
        //    return null;
        //}

        private static void GenerateBitmaps()
        {
            ArrayList carList=new ArrayList();
            Bitmap bmpTmp = new Bitmap(500, 500);
            Graphics gra = Graphics.FromImage(bmpTmp);
            Pen pen = new Pen(Color.Black, 2);
            Brush brushBlack = new SolidBrush(Color.Black);
            Brush brushRed = new SolidBrush(Color.Red);
            Brush brushWhite = new SolidBrush(Color.White);
            string host = "localhost:6379";
            //取连接对象
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(host);
            //取得DB对象
            IDatabase db= redis.GetDatabase();
            //////////////////
            while (true)
            {
                int AllBitMapsCount = bitmapList.Count;
                if (AllBitMapsCount>= 10)
                {
                    Thread.Sleep(50);
                    continue;
                }
                else {;}
                gra.Clear(Color.Gray);
                int maxCarCount;
                int.TryParse(db.StringGet("carMax"), out maxCarCount);
                carList.Clear();
                for (int carIndex = 1; carIndex < maxCarCount + 1; carIndex++)
                {
                    string psX = db.HashGet("Car" + carIndex + ":Position", "x");
                    string psY = db.HashGet("Car" + carIndex + ":Position", "y");
                    carList.Add(new Car(psX, psY));
                }
                //准备好所有的基本绘图信息
                for (int visitedX = 0; visitedX < 50; visitedX += 1)
                {
                    for (int visitedY = 0; visitedY < 50; visitedY += 1)
                    {
                        if (db.StringGetBit("mapView", visitedX * 50 + visitedY))
                        {
                            gra.FillEllipse(brushWhite, visitedY * 10 - 50, visitedX * 10 - 50, 100, 100);//刮奖
                                                                                                          //gra.DrawLine(pen, visitedY * 10, visitedX * 10, visitedY * 10 + 1, visitedX * 10);//路径点
                        }
                    }
                }//刮奖
                for (int visitedX = 0; visitedX < 50; visitedX++)
                {
                    for (int visitedY = 0; visitedY < 50; visitedY++)
                    {
                        if (db.StringGetBit("mapVisited", visitedX * 50 + visitedY))
                        {
                            //gra.FillEllipse(brushWhite, visitedY * 10 - 50, visitedX * 10 - 50, 100, 100);//刮奖
                            gra.DrawLine(pen, visitedY * 10, visitedX * 10, visitedY * 10 + 1, visitedX * 10);//路径点
                        }
                        if (db.StringGetBit("mapDetected", visitedX * 50 + visitedY))
                        {
                            gra.FillRectangle(brushBlack, visitedY * 10, visitedX * 10, 10, 10);
                        }
                    }
                }//轨迹+路障
                 //刮奖区上面刮开+画障碍+画路径点↑
                foreach (Car car in carList)
                {
                    int carX = car.posiXInt();
                    int carY = car.posiYInt();
                    gra.FillRectangle(brushRed, carY * 10, carX * 10, 5, 5);
                }
                bitmapList.Add(bmpTmp);
                //redis.Close();
                Thread.Sleep(80);
            }

            
        }

        private void wipFresher_Click(object sender, EventArgs e)
        {
            if (bitmapList.Count<=0)
            {
                label1.Text = "未取得";
                return;
            }
            if ((Bitmap)bitmapList[0]== null)
            {
                label1.Text="未取得";
                return;
            }
            else
            {
                Bitmap bmp4World = (Bitmap)bitmapList[0];
                bitmapList.Remove(bmp4World);
                this.displayArea.Image = bmp4World;
                label1.Text = "已经显示";
            }
        }

        public void printBlackBoard()
        {
            int[,] mapDetected = new int[50, 50];
            int[,] mapVisited = new int[50, 50];
            ArrayList carList = new ArrayList();
            //循环更新界面20次，每次睡眠200毫秒，
            for (int i = 0; i < 200; i++)
            {
                int maxCarCount;
                int.TryParse(db.StringGet("carMax"), out maxCarCount);
                carList.Clear();
                for (int carIndex = 1; carIndex < maxCarCount+1; carIndex++)
                {
                    string psX = db.HashGet("Car" + carIndex + ":Position", "x");
                    string psY = db.HashGet("Car" + carIndex + ":Position", "y");
                    carList.Add(new Car(psX,psY));
                }
                //准备好所有的基本绘图信息

                Invoke(new EventHandler(delegate
                {
                    //graphic.FillRectangle(brushBlack, rectangleBack);
                    //刮奖区
                    for (int visitedX = 0; visitedX < 50; visitedX++)
                    {
                        for (int visitedY = 0; visitedY < 50; visitedY++)
                        {
                            if (db.StringGetBit("mapView", visitedX * 50 + visitedY))
                            {
                                graphic.FillEllipse(brushWhite,visitedY*10-50,visitedX*10-50,100,100);//刮奖
                            }
                            
                        }
                    }//刮开刮奖区
                    for (int visitedX = 0; visitedX < 50; visitedX++)
                    {
                        for (int visitedY = 0; visitedY < 50; visitedY++)
                        {
                            if (db.StringGetBit("mapDetected", visitedX * 50 + visitedY))
                            {
                                graphic.FillRectangle(brushBlack, visitedY * 10, visitedX * 10, 10, 10);
                            }//路障
                            if (db.StringGetBit("mapVisited", visitedX * 50 + visitedY))
                            {
                                graphic.DrawLine(pen, visitedY * 10, visitedX * 10, visitedY * 10 + 1, visitedX * 10);//路径点
                            }//路径点
                        }
                    }//画路障和路径点
                    //for (int visitedX = 0; visitedX < 50; visitedX++)
                    //{
                    //    for (int visitedY = 0; visitedY < 50; visitedY++)
                    //    {
                    //        if (db.StringGetBit("mapVisited", visitedX * 50 + visitedY))
                    //        {
                    //            graphic.DrawLine(pen, visitedY * 10, visitedX * 10, visitedY * 10 + 1, visitedX * 10);//路径点
                    //        }
                    //    }
                    //}//画轨迹点

                    //刮奖区上面刮开+画障碍+画路径点+画路障
                    foreach (Car car in carList)
                    {
                        int carX = car.posiXInt();
                        int carY = car.posiYInt();
                        //graphic.DrawLine(pen, carY * 10, carX * 10, carY * 10 + 1, carX* 10);//路径点
                        graphic.FillRectangle(brushRed, carY*10,carX*10,5,5);
                    }
                    Thread.Sleep(50);
                    //画车



                }));
                //Thread.Sleep(2);
            }
        }

    }
}
