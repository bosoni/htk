// game-test (c) by mjt
using System;
using System.Collections.Generic;
using OpenTK;

namespace GameTest
{
    public class Animation
    {
        Actor actor;
        List<Vector3> points = new List<Vector3>();
        int curPos = 0;
        float curY = 0;
        Vector3 dir;

        public Animation() { }
        public Animation(Actor actor)
        {
            AddActor(actor);
        }
        public void AddActor(Actor actor)
        {
            this.actor = actor;
        }

        public void Add(Vector3 point)
        {
            points.Add(point);
        }
        public void Clear()
        {
            points.Clear();
            curPos = 0;
            dir = Vector3.Zero;
        }

        /// <summary>
        /// päivittää animaatiota että actor liikkuu pisteestä A->B->C jne
        /// 
        /// speed vaikuttaa liikkumisnopeuteen, animTimeSpd vaikuttaa animoinnin nopeuteen
        /// 
        /// jos liikuttiin onnistuneesti, palauta true, muuten false.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="animTimeSpd"></param>
        /// <returns></returns>
        public bool Update(float speed, float animTimeSpd, Map map, ref List<Actor> actors)
        {
            if (points.Count == 2)
            {
                return MoveActorInMap(speed, animTimeSpd, ref map, ref actors);
            }

            if (points.Count < 2) return false;
            if (curPos >= points.Count - 2) return false;

            // lasketaan actorille suunta ja asento
            dir = points[curPos + 1] - points[curPos];
            float ry = (float)Math.Atan2(dir.X, dir.Z);

            ry *= 180 / 3.1415f; // arvo välille  -180 -> 180
            //float angle1 = (ry - curY);
            //curY += angle1 / 4;
            //curY += 180;
            //actor.Rotation.Y = curY;
            actor.Rotation.Y = ry;
        
            Vector3 newPos = points[curPos] + dir * curTime;
            // tsekkaa liikutaanko lähelle toista actoria, jos niin pysähdy
            if (actors != null)
            {
                foreach (Actor act in actors)
                {
                    if (act == actor) continue;

                    Vector3 len = act.Position - newPos;
                    if (len.LengthFast < 0.5f)
                    {
                        return false;
                    }
                }
            }

            actor.Position = newPos;
            curTime += speed;
            if (curTime >= 1)
            {
                curTime = 0;
                curPos++;
                actor.Position = points[curPos];
            }
 
            actor.AnimTime += speed * animTimeSpd;
            actor.Update();

            return true;
        }
        
        float curTime = 0;


        /// <summary>
        /// liikuttaa actoria kartalla pisteestä A pisteeseen B. 
        /// jos liikuttiin onnistuneesti, palauta true, muuten false.
        /// </summary>
        public bool MoveActorInMap(float speed, float animTimeSpd, ref Map map, ref List<Actor> actors)
        {
            if (points.Count < 2) return false;
            if (curPos >= points.Count - 1) return false;

            // lasketaan actorille suunta ja asento
            dir = points[curPos + 1] - points[curPos];
            float ry = (float)Math.Atan2(dir.X, dir.Z);

            ry *= 180 / 3.1415f; // arvo välille  -180 -> 180
            ry += 180; // välille 0-360
            float angle1 = (ry - curY);
            curY += angle1 / 4;
            curY += 180;
            actor.Rotation.Y = curY + 180;

            // tsekkaa jos ollaan tarpeeks lähellä haluttuja loppukoordinaatteja
            Vector3 len = points[curPos + 1] - actor.Position;
            if (len.LengthFast < 0.1f)
            {
                actor.UpdateTransform();
                Clear();
                return false;
            }

            // tsekkaa jos ollaan seinän vieressä niin pysähdytään
            dir.Normalize();
            Vector3 newPos = actor.Position + dir * speed;
            if (map.IsFreePlace(map.GetMapX(newPos.X), map.GetMapY(newPos.Z)) == false)
            {
                actor.UpdateTransform();
                Clear();
                return false;
            }

            // tsekkaa liikutaanko lähelle toista actoria, jos niin pysähdy
            if (actors != null)
            {
                foreach (Actor act in actors)
                {
                    if (act == actor) continue;

                    len = act.Position - newPos;
                    if (len.LengthFast < 0.5f)
                    {
                        actor.UpdateTransform();
                        Clear();
                        return false;
                    }
                }
            }

            actor.Position = newPos;
            actor.AnimTime += speed * animTimeSpd;
            actor.Update();
            return true;
        }


    }

}
