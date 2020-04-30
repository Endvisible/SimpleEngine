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
        private int Resize;
        private int ColumnCount;

        public bool IsCentered;
        public List<Tile> Tiles;
        public Dictionary<string, object> Actors;

        // return top-left corner of entire map
        public Vector2 Position { get { 
            return this.Tiles[0].Position; 
        } }

        // return center of map
        public Vector2 Center { get
        {
                return this.Position + SceneSize / 2 - (IsCentered ? Size * Resize / 2 : new Vector2(0));
        } }

        // return pixel-width of full tile map
        public int Width { get { 
            return (int)((Layout.OrderBy(x => x.Length)).Last().Length
                          * Size.X
                          * Resize);
        } }

        // return pixel height of full tile map
        public int Height { get {
            return (int)(Layout.Length
                         * Size.Y
                         * Resize);
        } }

        public Vector2 SceneSize { get { return new Vector2(Width, Height); } }


        public Scene(
            Texture2D SpriteSheet,
            Vector2 Size,
            float[][] Layout,

            int Resize = 1,
            int ColumnCount = 1,
            bool IsCentered = false)
        {
            this.SpriteSheet = SpriteSheet;
            this.Size = Size;
            this.Layout = Layout;

            this.Resize = Resize;
            this.IsCentered = IsCentered;
            this.ColumnCount = ColumnCount;

            Tiles = new List<Tile>();
            Actors = new Dictionary<string, object>();

            int colNumber = 0;
            int rowNumber = 0;
            foreach (float[] row in this.Layout)
            {
                foreach (float col in row)
                {
                    string colString = string.Format("{0:N4}", col);
                    string decimalPoints = colString
                                               .ToString()
                                               .Substring(colString.IndexOf(".")
                                               + 1);

                    bool isBoundary = true;

                    // get all decimal points as strings and add them to a list
                    List<string> decimalPointsAsStrings = new List<string>();
                    for (int i = 0; i < decimalPoints.Length; i++)
                        decimalPointsAsStrings.Add(decimalPoints[i].ToString());

                    // test if all decimal points are zero
                    if (decimalPointsAsStrings.All(x => x == "0")) isBoundary = false;

                    int[] collisionBounds = (from item in decimalPoints
                                             select Int32.Parse(item.ToString()))
                                             .ToArray<int>();

                    Tiles.Add(
                        new Tile(
                            SpriteSheet: this.SpriteSheet,
                            Position: new Vector2(
                                (int)colNumber * Size.X * Resize
                                     - (IsCentered ? Width / 2 : 0),
                                (int)rowNumber * this.Size.Y * this.Resize
                                     - (IsCentered ? Height / row.Count() + Size.Y * Resize / 2 : 0)),
                            Size: this.Size,

                            Resize: this.Resize,
                            ColumnCount: this.ColumnCount,
                            Frame: (int)col,
                            CollisionBounds: collisionBounds,
                            IsBoundary: isBoundary,
                            IsCentered: this.IsCentered ? true : false));
                    colNumber++;
                }
                colNumber = 0;
                rowNumber++;
            }
            foreach (Tile tile in Tiles) tile.Update();
        }

        public void Offset(Vector2 offset)
        {
            foreach (Tile tile in Tiles)
            {
                tile.Position += offset;
                tile.Update();
            }
        }

        public void Dispose()
        {
            SpriteSheet.Dispose();
            foreach (Tile tile in this.Tiles) tile.SpriteSheet.Dispose();
        }

        public void Draw(SpriteBatch spriteBatch)
        { foreach (Tile tile in Tiles) tile.Draw(spriteBatch); }
    }
}
