/*
 * Htk example (c) mjt, 2011-2014
 * 
 * 
 */

// testing:  render scene, use gl*, render particles


using Horde3DNET;
using Horde3DNET.Utils;
using OpenTK;
using OpenTK.Input;
using Htk;

namespace Test
{
    public class Test_GL : BaseGame
    {
        Light light = new Light();
        Particles particles;
        Model floor;

        public override void Init()
        {
            particles = Particles.Load("particleSys1");
            particles.Position = new Vector3(0, 1, 0);
            particles.Rotation = new Vector3(90, 0, 0);
            particles.Update();

            floor = Model.Load("platform");
            Camera.SetSkyBox("skybox");

            Camera.Position.Y = 2;
            Camera.Position.Z = 10;
            light.Create(h3d.H3DRootNode, new Vector3(0, 10, 0), new Vector3(-90, 0, 0));

            base.Init();
        }

        public override void Update(float time)
        {
            float spd = (float)time * 10;
            if (Keyboard[Key.ShiftLeft] || Keyboard[Key.ShiftRight]) spd *= 2;
            if (Keyboard[Key.W]) Camera.Move(spd);
            if (Keyboard[Key.S]) Camera.Move(-spd);
            if (Keyboard[Key.A]) Camera.Strafe(-spd);
            if (Keyboard[Key.D]) Camera.Strafe(spd);
            if (Mouse[MouseButton.Left])
            {
                Camera.Rotation.Y -= Mouse.X - oldMouseX;
                Camera.Rotation.X -= Mouse.Y - oldMouseY;
            }

            // Animate particle system
            particles.Update(time);

            base.Update(time);
        }

        public override void Render(float time)
        {
            Camera.Update();


            // hide only particles
            h3d.setNodeFlags(particles.Node, (int)h3d.H3DNodeFlags.Inactive, true);
            h3d.render(Camera.Node);
            h3d.setNodeFlags(particles.Node, 0, true); // back to visible


            // draw GL stuff
            // forward.pipeline.xml pitää muuttaa ettei clearata 
            // (ettei jälkimmäinen h3d.render() tyhjennä jo rendattua graffaa:
            //     <ClearTarget depthBuf="true" colBuf0="false" />


            // hide everything
            h3d.setNodeFlags(h3d.H3DRootNode, (int)h3d.H3DNodeFlags.Inactive, true);
            // set only particles visible
            h3d.setNodeFlags(particles.Node, 0, true);
            h3d.render(Camera.Node);

            h3d.setNodeFlags(h3d.H3DRootNode, 0, true); // all visible
        }
    }
}
