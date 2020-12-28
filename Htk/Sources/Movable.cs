/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System;
using Horde3DNET;
using OpenTK;

namespace Htk
{
    public class Movable : IDisposable
    {
        static protected int _count = 0;
        bool disposed = false;
        public const float PiOver180 = (float)(Math.PI / 180.0);

        protected String name = "";
        public int Node = -1;

        public Vector3 Position = new Vector3(), Rotation = new Vector3(), Scale = new Vector3(1, 1, 1);

        //Implement IDisposable.
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free managed objects
                }

                // Free unmanaged objects
                if (Node != -1)
                {
                    if (BaseGame.Running)
                    {
                        h3d.removeNode(Node);
                    }
                    Node = -1;
                }
                disposed = true;
            }
        }
        ~Movable()
        {
            Dispose(false);
        }

        public Vector3 GetDirection(float scale)
        {
            Vector3 rotation = Rotation * PiOver180;
            Vector3 direction;
            direction.X = (float)Math.Sin(rotation.Y) * (float)Math.Cos(-rotation.X) * scale;
            direction.Y = (float)Math.Sin(-rotation.X) * scale;
            direction.Z = (float)Math.Cos(rotation.Y) * (float)Math.Cos(-rotation.X) * scale;
            return -direction;
        }

        /// <summary>
        /// Updates model's position, rotation and scale. 
        /// Call this every time something changes.
        /// </summary>
        public virtual void Update()
        {
            h3d.setNodeTransform(Node,
                Position.X, Position.Y, Position.Z,
                Rotation.X, Rotation.Y, Rotation.Z,
                Scale.X, Scale.Y, Scale.Z);
        }

        public void Move(float spd)
        {
            Vector3 rotation = Rotation * PiOver180;
            Position.X -= (float)Math.Sin(rotation.Y) * (float)Math.Cos(-rotation.X) * spd;
            Position.Y -= (float)Math.Sin(-rotation.X) * spd;
            Position.Z -= (float)Math.Cos(rotation.Y) * (float)Math.Cos(-rotation.X) * spd;
        }

        public void Strafe(float spd)
        {
            Vector3 rotation = Rotation * PiOver180;
            Position.X -= (float)Math.Sin(rotation.Y - 90f * PiOver180) * spd;
            Position.Z -= (float)Math.Cos(rotation.Y - 90f * PiOver180) * spd;
        }

        public void Strafe(float spd, float angle)
        {
            angle *= PiOver180;
            Vector3 rotation = Rotation * PiOver180;
            Position.X -= (float)Math.Sin(rotation.Y + angle) * spd;
            Position.Z -= (float)Math.Cos(rotation.Y + angle) * spd;
        }

    }

}
