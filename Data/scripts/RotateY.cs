using System;
using OpenTK;

public class CSScript
{
    public static void Main()
    {
        
    }

    public void Update(object p)
    {
        HSceneEditor.Obj o = (HSceneEditor.Obj)p;

        o.rot.Y += 0.1f;
    }

}
