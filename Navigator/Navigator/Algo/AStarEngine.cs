using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AStar
{
    class AStarEngine
    {
        private ArrayList _openList = new ArrayList();
        private ArrayList _closedList = new ArrayList();
        private AStarNode _resultNode = null;

        public AStarEngine() { }

        public AStarNode ResultNode
        {
            get { return _resultNode; }
        }

        public bool Found
        {
            get { return _resultNode != null; }
        }

        public bool Execute(AStarNode start, AStarNode goal)
        {
            _openList.Clear(); _closedList.Clear();

            start.G = 0;
            start.H = start.GoalDistEstimate(goal);
            _openList.Add(start);
            while (_openList.Count > 0)
            {
                AStarNode node = getBestNode();
                if (node.EqualTo(goal))
                {
                    _resultNode = node;
                    return true;
                }

                ArrayList successors = new ArrayList();
                node.Propagate(successors);
                foreach (AStarNode sn in successors)
                {
                    int no = findNodeIndex(_openList, sn);
                    int nc = findNodeIndex(_closedList, sn);
                    float newg = node.G + node.Cost(sn);

                    if (no >= 0 && ((AStarNode)_openList[no]).G <= newg)
                        continue;
                    if (nc >= 0 && ((AStarNode)_closedList[nc]).G <= newg)
                        continue;

                    if (nc >= 0) _closedList.RemoveAt(nc);
                    if (no >= 0)
                    {
                        AStarNode nt = (AStarNode)_openList[no];
                        nt.G = newg;
                        nt.H = nt.GoalDistEstimate(goal);
                        nt.Parent = node;
                    }
                    else
                    {
                        sn.G = newg;
                        sn.H = sn.GoalDistEstimate(goal);
                        sn.Parent = node;
                        _openList.Add(sn);
                    }
                }

                _closedList.Add(node);
            }

            return false;
        }

        private AStarNode getBestNode()
        {
            AStarNode node = null;

            for (int i = 0; i < _openList.Count; i++)
            {
                if ((node != null) && node.F > ((AStarNode)_openList[i]).F)
                    node = (AStarNode)_openList[i];

                if (i == 0) node = (AStarNode)_openList[0];
            }

            if (node != null) _openList.Remove(node);

            return node;
        }

        private int findNodeIndex(ArrayList list, AStarNode node)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (((AStarNode)list[i]).EqualTo(node))
                    return i;
            }

            return -1;
        }
    }
}
