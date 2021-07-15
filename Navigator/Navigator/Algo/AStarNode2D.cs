using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AStar
{
    /// <summary>
    /// 与地图相关的A*节点
    /// </summary>
    class AStarNode2D : AStarNode
    {
        private int _x, _y;
        private Map _map;

        public AStarNode2D(int x, int y, Map map)
        {
            this._x = x;
            this._y = y;
            this._map = map;
        }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public override bool EqualTo(AStarNode node)
        {
            AStarNode2D n = (AStarNode2D)node;
            return (n.X == this.X) && (n.Y == this.Y);
        }

        public override void Propagate(ArrayList successors)
        {
            AddSuccessor(_x, _y - 1, successors);
            AddSuccessor(_x + 1, _y - 1, successors);
            AddSuccessor(_x + 1, _y, successors);
            AddSuccessor(_x + 1, _y + 1, successors);
            AddSuccessor(_x, _y + 1, successors);
            AddSuccessor(_x - 1, _y + 1, successors);
            AddSuccessor(_x - 1, _y, successors);
            AddSuccessor(_x - 1, _y - 1, successors);
        }

        private void AddSuccessor(int x, int y, ArrayList successors)
        {
            int cost = _map.GetMapData(x, y);

            // 代价是-1，则路径不同
            if (cost == -1) return;

            AStarNode2D node = new AStarNode2D(x, y, _map);
            AStarNode2D p = (AStarNode2D)this.Parent;
            while (p != null)
            {
                if (node.EqualTo(p)) return;
                p = (AStarNode2D)p.Parent;
            }

            successors.Add(node);
        }
        public override float GoalDistEstimate(AStarNode node)
        {
            return Cost(node);
        }

        public override float Cost(AStarNode node)
        {
            AStarNode2D n = (AStarNode2D)node;
            // delta X
            int xd = n.X - this.X;
            // delta Y
            int yd = n.Y - this.Y;

            // 计算几何距离
            return (float)Math.Sqrt(xd * xd + yd * yd);
        }
    }
}
