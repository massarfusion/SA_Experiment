using AStar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Navigator
{
    class Calc
    {
        public ArrayList finalResult(int[,] request,int startx,int starty,int endx,int endy)
        {
            Map map = new Map();
            map.SetNewMap(request);
            //起始节点
            AStarNode2D start = new AStarNode2D(startx, starty, map);
            // 目标节点
            AStarNode2D goal = new AStarNode2D(endx, endy, map);
            // A* 算法引擎
            AStarEngine engine = new AStarEngine();

            // 初始化执行步骤序列
            ArrayList solution = new ArrayList();

            //路径搜索
            if (engine.Execute(start, goal))
            {
                #region 准备构造路径
                //构造路径
                AStarNode2D node = (AStarNode2D)engine.ResultNode;
                while (node != null)
                {
                    solution.Insert(0, node);
                    node = (AStarNode2D)node.Parent;
                }
                // 构造路径结束，此时solution中就是行走路径
                #endregion


                Console.WriteLine("Path found:");
                return solution;
                
            }
            else
            {
                Console.WriteLine("Unable to find a path.");
                return null;
            }


        }
    }
}
