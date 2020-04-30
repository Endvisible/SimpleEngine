using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SimpleEngine
{
    public class KeyHandler
    {
        private KeyboardState CurrentKeyState, PreviousKeyState;
        public List<string> Directions;


        public KeyHandler()
        {
            Directions = new List<string>();
        }


        public KeyboardState GetState()
        {
            PreviousKeyState = CurrentKeyState;
            CurrentKeyState = Keyboard.GetState();
            return CurrentKeyState;
        }


        public bool Get(Keys key) 
        { return Keyboard.GetState().IsKeyDown(key); }


        public bool KeyPressed(Keys key)
        { return CurrentKeyState.IsKeyDown(key) && PreviousKeyState.IsKeyUp(key); }


        public bool KeyReleased(Keys key)
        { return CurrentKeyState.IsKeyUp(key) && PreviousKeyState.IsKeyDown(key); }

        public void Update()
        {

            #region Handle Directions
            // if W is being held with no conflicts
            if (Get(Keys.W) 
                && !Get(Keys.S) 
                && !(Get(Keys.A) && Get(Keys.D)))
                { if (!(Directions.Contains("up"))) Directions.Add("up"); }

            // if W is released or is not held
            else { if (Directions.Contains("up")) Directions.Remove("up"); }

            // same formula for A, S, D
            if (Get(Keys.A) && !Get(Keys.D) && !(Get(Keys.W) && Get(Keys.S)))
                { if (!(Directions.Contains("left"))) Directions.Add("left"); }
            else { if (Directions.Contains("left")) Directions.Remove("left"); }

            if (Get(Keys.S) && !Get(Keys.W) && !(Get(Keys.A) && Get(Keys.D)))
                { if (!(Directions.Contains("down"))) Directions.Add("down"); }
            else { if (Directions.Contains("down")) Directions.Remove("down"); }

            if (Get(Keys.D) && !Get(Keys.A) && !(Get(Keys.W) && Get(Keys.S)))
                { if (!(Directions.Contains("right"))) Directions.Add("right"); }
            else { if (Directions.Contains("right")) Directions.Remove("right"); }
            #endregion

        }

    }
}
