using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEngine
{
    public class Camera
    {
        public Matrix Transform;
        private Viewport View;
        private Vector2 Center;

        private float zoom = 1;

        public float Zoom
        {
            get { return zoom; }
            set { zoom = (value < .1f ? .1f : value); }
        }

        public Camera(Viewport newView)
        {
            View = newView;
        }

        public void Update(Sprite target)
        {
            Center = -target.HitBox.Center.ToVector2();
            Transform =
                Matrix.CreateTranslation(new Vector3(Center.X, Center.Y, 0))
                * Matrix.CreateScale(new Vector3(Zoom, Zoom, 0))
                * Matrix.CreateTranslation(new Vector3(View.Width / 2, View.Height / 2, 0));
        }
    }


}
