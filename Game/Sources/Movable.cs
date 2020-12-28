// game-test (c) by mjt
using System;
using Horde3DNET;
using OpenTK;

namespace GameTest
{
    public class Movable
    {
        const float PiOver180 = (float)(Math.PI / 180.0);
        public Vector3 Position, Rotation;
        public int Node;
        public float AnimTime = -1;

        public Vector3 GetDirection(float scale)
        {
            Vector3 rotation = Rotation * PiOver180;
            Vector3 direction;
            direction.X = (float)Math.Sin(rotation.Y) * (float)Math.Cos(-rotation.X) * scale;
            direction.Y = (float)Math.Sin(-rotation.X) * scale;
            direction.Z = (float)Math.Cos(rotation.Y) * (float)Math.Cos(-rotation.X) * scale;
            return -direction;
        }

        public void UpdateAnim()
        {
            if (AnimTime == -1) return;
            h3d.setModelAnimParams(Node, 0, AnimTime, 1.0f);
            h3d.updateModel(Node, (int)(h3d.H3DModelUpdateFlags.Animation));
        }

        public void Update()
        {
            UpdateAnim();
            UpdateTransform();
        }

        public void UpdateTransform()
        {
            h3d.setNodeTransform(Node,
                Position.X, Position.Y, Position.Z,
                Rotation.X, Rotation.Y, Rotation.Z,
                1, 1, 1);
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
