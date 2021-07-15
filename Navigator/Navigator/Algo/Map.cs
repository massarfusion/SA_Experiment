using System;
using System.Collections.Generic;
using System.Text;

namespace AStar
{
    class Map
    {
        /// <summary>
        /// 地图大小，10 表示 10  *  10 矩阵，如果要嵌入实验则需按照你的设计修改
        /// </summary>
        public const int MapSize = 50;

        /// <summary>
        /// -1：墙，不能走  
        /// 0：可以走 
        /// 其它值：本算法未讨论，如需使用则需修改AStarNode2D类的Cost函数
        /// </summary>
        static int[,] MapData = 
        {
            { 0,-1, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0,-1, 0, 0,-1,-1,-1,-1, 0, 0 },
            { 0,-1, 1, 0,-1, 0, 2,-1, 0, 0 },
            { 0,-1, 0, 0,-1, 0, 0, 0, 0, 0 },
            { 0,-1, 0, 0,-1, 0, 0, 0, 0, 0 },
            { 0,-1,-1,-1,-1, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        public int GetMapData(int x, int y)
        {
            //安全性检查
            if (x < 0 || x > MapSize - 1 || y < 0 || y > MapSize-1)
                return -1;
            else
                return MapData[y, x];
        }
        public void SetNewMap(int [,] newVis)
        {
            MapData = newVis;
        }
    }
}
