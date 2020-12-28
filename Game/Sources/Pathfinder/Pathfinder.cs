using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpatialAStar
{
    public class MyPathNode : IPathNode<Object>
    {
        public Int32 X { get; set; }
        public Int32 Y { get; set; }
        public Boolean IsWall { get; set; }

        public bool IsWalkable(Object unused)
        {
            return !IsWall;
        }
    }

    public class MySolver<TPathNode, TUserContext> : SpatialAStar<TPathNode, TUserContext> where TPathNode : IPathNode<TUserContext>
    {
        protected override Double Heuristic(PathNode inStart, PathNode inEnd)
        {
            return Math.Abs(inStart.X - inEnd.X) + Math.Abs(inStart.Y - inEnd.Y);
        }

        protected override Double NeighborDistance(PathNode inStart, PathNode inEnd)
        {
            return Heuristic(inStart, inEnd);
        }

        public MySolver(TPathNode[,] inGrid)
            : base(inGrid)
        {
        }
    }

    public class PathFinder
    {
        int width, height;
        MyPathNode[,] grid;
        public PathFinder(int width, int height, byte[, ,] map)
        {
            this.grid = new MyPathNode[width, height];
            this.width = width;
            this.height = height;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool block = false;

                    if (map[x, y, 0] != 0) block = true;
                    if (map[x, y, 1] != 0) block = true;
                    if (map[x, y, 2] != 0) block = true;

                    grid[x, y] = new MyPathNode()
                    {
                        IsWall = block,
                        X = x,
                        Y = y,
                    };
                }
            }
        }

        public IEnumerable<MyPathNode> Search(int startx, int starty, int endx, int endy)
        {
            MySolver<MyPathNode, Object> aStar = new MySolver<MyPathNode, Object>(grid);
            IEnumerable<MyPathNode> path = aStar.Search(new Point(startx, starty), new Point(endx, endy), null);
            return path;
        }
    }
}
