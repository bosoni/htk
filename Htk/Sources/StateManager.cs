/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System.Collections.Generic;

namespace Htk
{
    public static class StateManager
    {
        class State
        {
            public BaseGame game;
            public bool active;
            public State(BaseGame gameState, bool actived)
            {
                game = gameState;
                active = actived;
            }
        }
        static List<State> states = new List<State>();


        public static void Clear()
        {
            foreach (State s in states)
                if (s.active)
                    s.game.Dispose();
            states.Clear();
        }

        public static void Add(BaseGame game, bool active)
        {
            State thisState = new State(game, active);
            states.Add(thisState);
            if (active == true) thisState.game.Init();
        }

        public static void Remove(BaseGame game)
        {
            foreach (State s in states)
                if (s.game == game)
                    states.Remove(s);
        }

        public static void ChangeSettings(BaseGame game, bool active)
        {
            foreach (State s in states)
            {
                if (s.game == game)
                {
                    if (s.active == false && active == true) s.game.Init();
                    else if (s.active == true && active == false) s.game.Dispose();
                    s.active = active;
                }
            }
        }

        public static bool IsAnyStateActive()
        {
            foreach (State s in states)
                if (s.active == true)
                    return true;

            return false;
        }

        public static void Update(float time)
        {
            foreach (State s in states)
                if (s.active == true)
                    s.game.Update(time);
        }

        public static void Render(float time)
        {
            foreach (State s in states)
                if (s.active == true)
                    s.game.Render(time);
        }

    }
}
