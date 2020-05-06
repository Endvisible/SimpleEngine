using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleEngine
{
    public class Scene
    {
        private Texture2D SpriteSheet;
        private float[][] Layout;
        private Vector2 Size;

        private int ColumnCount;

        public bool IsCentered;
        public List<Tile> Tiles;
        public Dictionary<string, object> Actors;

        public List<Rectangle> Boundaries;

        // return top-left corner of entire map
        public Vector2 Position { get { 
            return this.Tiles[0].Position; 
        } }

        // return center of map
        public Vector2 Center { get
        {
                return this.Position + SceneSize / 2 - (IsCentered ? Size / 2 : new Vector2(0));
        } }

        // return pixel-width of full tile map
        public int Width { get { 
            return (int)((Layout.OrderBy(x => x.Length)).Last().Length
                          * Size.X);
        } }

        // return pixel height of full tile map
        public int Height { get {
            return (int)(Layout.Length
                         * Size.Y);
        } }

        public Vector2 SceneSize { get { return new Vector2(Width, Height); } }


        public Scene(
            Texture2D SpriteSheet,
            Vector2 Size,
            float[][] Layout,

            int ColumnCount = 1,
            bool IsCentered = false)
        {
            this.SpriteSheet = SpriteSheet;
            this.Size = Size;
            this.Layout = Layout;

            this.IsCentered = IsCentered;
            this.ColumnCount = ColumnCount;

            Tiles = new List<Tile>();
            Actors = new Dictionary<string, object>();
            Boundaries = new List<Rectangle>();

            int colNumber = 0;
            int rowNumber = 0;
            foreach (float[] row in this.Layout)
            {
                foreach (float col in row)
                {
                    Tiles.Add(
                        new Tile(
                            SpriteSheet: this.SpriteSheet,
                            Position: new Vector2(
                                (int)colNumber * Size.X
                                     - (IsCentered ? Width / 2 - Size.X / 2 : 0),
                                (int)rowNumber * this.Size.Y
                                     - (IsCentered ? Height / 2 - Size.Y / 2 : 0)),
                            Size: this.Size,

                            ColumnCount: this.ColumnCount,
                            Frame: (int)col,
                            IsCentered: this.IsCentered ? true : false));
                    colNumber++;
                }
                colNumber = 0;
                rowNumber++;
            }
            foreach (Tile tile in Tiles) tile.Update();
        }

        #region Functions

        public void AddBoundary(Rectangle boundaryRect)
        { Boundaries.Add(boundaryRect); }

        #endregion

        public void Dispose()
        {
            SpriteSheet.Dispose();
            foreach (Tile tile in this.Tiles) tile.SpriteSheet.Dispose();
        }

        public void Draw(SpriteBatch spriteBatch)
        { foreach (Tile tile in Tiles) tile.Draw(spriteBatch); }
    }
}
