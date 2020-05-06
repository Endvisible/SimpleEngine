using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleEngine
{
    #region Tile

    public class Tile
    {

        #region Tile Parameters
        public Texture2D SpriteSheet; // spritesheet image

        protected int ColumnCount;       // number of columns in spritesheet
        protected int Frame;             // frame number

        protected bool IsCentered;       // tile origin is center

        public Vector2 Position;         // position on screen

        public Vector2 Size; // size in pixels
        
        public Rectangle FrameSource; // sourceRectangle
        public Rectangle FrameOutput; // destinationRectangle


        public Rectangle Rectangle
        { get {
            return new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)Size.X,
                (int)Size.Y);
        } }

        public Vector2 Center
        { get {
            return new Vector2(
                Rectangle.Width / 2,
                Rectangle.Height / 2);
        } }

        public int Width { get { return this.Rectangle.Width; } }
        public int Height { get { return this.Rectangle.Height; } }
        #endregion

        public Tile(
            Texture2D SpriteSheet,
            Vector2 Position,
            Vector2 Size,

            int Frame = 0,
            int ColumnCount = 1,
            bool IsCentered = false
            )
        {
            this.SpriteSheet = SpriteSheet;
            this.Size = Size;
            this.Position = Position;

            this.ColumnCount = ColumnCount;
            this.Frame = Frame;

            this.IsCentered = IsCentered;
        }

        #region Functions

        // get portion of image from frame number
        protected Vector2 GetSprite(int index)
        {
            int yIndex = 0;

            while (index >= ColumnCount)
            { 
                index -= ColumnCount;
                yIndex += 1; 
            }

            return new Vector2(
                index * Size.X,
                yIndex * Size.Y
                );
        }
        #endregion

        // update tile
        public void Update()
        {
            FrameSource = new Rectangle(
                (int)GetSprite(Frame).X,
                (int)GetSprite(Frame).Y,
                (int)Size.X,
                (int)Size.Y);

            FrameOutput = new Rectangle(
                (int)Rectangle.X,
                (int)Rectangle.Y,
                (int)Rectangle.Width,
                (int)Rectangle.Height);
        }

        // draw to screen
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                SpriteSheet,
                sourceRectangle: FrameSource,
                destinationRectangle: FrameOutput,
                origin: IsCentered ? Size / 2 : new Vector2(0, 0));
        }
    }

    #endregion

    #region Sprite
    public class Sprite : Tile
    {
        #region Sprite Parameters

        private Vector2 nextPosition; // used to move sprite

        private float SpeedMultiplier; // speed multiplier

        private string Facing;    // direction sprite is facing

        private Vector2 HitBoxSize; // size of hitbox

        private int FrameIndex = 0;            // animation frame counter
        private double FrameTime = 0;          // frame time counter
        private double FrameLength = 1f / 30f; // frame rate

        private List<string> Directions;   // keys held (in order)
        private Tuple<int[], int> CurrentAnimation; // animation currently playing
        private Scene Scene;               // scene in which sprite is an actor

        // all animations sprite can do -- add audio option (ex. footsteps, hits)?
        private Dictionary<string, Tuple<int[], int>> Animations =
            new Dictionary<string, Tuple<int[], int>>
            { 
                { "default", new Tuple<int[], int>
                    (new int[]{ 0 }, 1) }
            };

        public float Speed;           // movement speed
        public Rectangle HitBox;      // sprite hitbox

        #endregion

        public Sprite(
            Texture2D SpriteSheet,
            Vector2 Position,
            Vector2 Size,
            
            bool IsCentered = false,
            int ColumnCount = 1,
            Vector2 HitBoxSize = new Vector2(),

            int Speed = 60) 

            : base(SpriteSheet, Position, Size)
        {
            this.Position = Position;
            this.Size = Size;

            this.IsCentered = IsCentered;
            this.ColumnCount = ColumnCount;
            this.HitBoxSize = HitBoxSize;

            this.Speed = Speed;
            this.Directions = new List<string>(capacity: 4);

            SpeedMultiplier = 1;

            Facing = "down";

            if (!(HitBoxSize == default))
                this.HitBoxSize = SetHitBox(HitBoxSize);
            else this.HitBoxSize = SetHitBox(this.Size);

            CurrentAnimation = Animations["default"];
            Frame = 0;
        }

        #region Functions
        private Vector2 SetHitBox(Vector2 size)
        {
            return new Vector2(
                HitBoxSize.X = size.X,
                HitBoxSize.Y = size.Y);
        }

        public void AddAnimation(string animationName, Tuple<int[], int> animationComponents)
        { Animations.Add(animationName, animationComponents); }

        public void SetAnimation(string animationName)
        {
            CurrentAnimation = Animations[animationName];
            FrameLength = 1f / (float)CurrentAnimation.Item2;
            Frame = 0;
        }

        public void SetScene(Scene scene, string name)
        {
            this.Scene = scene;
            scene.Actors.Add(name, this);
        }

        public bool GetCollision(string direction, Vector2 position)
        {

            /*
            // adjust for sprite center
            position += IsCentered ? 
                new Vector2(0) :
                new Vector2(HitBox.Center.X - Position.X, HitBox.Center.Y - Position.Y);
            */

            int collisionBuffer = 2;
            Rectangle testSide = new Rectangle();
            if (direction == "up") testSide = new Rectangle(HitBox.Left, HitBox.Top, HitBox.Width, 1);
            if (direction == "down") testSide = new Rectangle(HitBox.Left, HitBox.Bottom + 1, HitBox.Width, 1);
            if (direction == "left") testSide = new Rectangle(HitBox.Left - 1, HitBox.Top, 1, HitBox.Height);
            if (direction == "right") testSide = new Rectangle(HitBox.Right + 1, HitBox.Top, 1, HitBox.Height);

            foreach (Rectangle boundary in this.Scene.Boundaries)
            {
                if (testSide.Intersects(boundary)) return true;
            }
            return false;
        }

        public void Move(GameTime gameTime, KeyHandler keyHandler)
        {
            // speed is multiplied if shift is held, then reset - otherwise, skip entirely
            if (keyHandler.Get(Keys.LeftShift)) SpeedMultiplier = 2;
            else if (SpeedMultiplier != 1) SpeedMultiplier = 1;

            // list of direction keys being held
            Directions = keyHandler.Directions;

            // first direction held gets priority (used for animation)
            string primaryDirection;
            if (keyHandler.Directions.Count > 0)
                primaryDirection = keyHandler.Directions[0];
            else primaryDirection = Facing;


            float movementSpeed = 
                Speed
                * (float)gameTime.ElapsedGameTime.TotalSeconds
                * SpeedMultiplier;


            nextPosition = Position;

            // if sprite is not moving, set animation to idle facing
            if (keyHandler.Directions.Count == 0) SetAnimation($"idle_{Facing}");

            // if sprite is moving, move and animate along facing
            else
            {

                if (Directions.Contains("up")) nextPosition.Y -= movementSpeed;
                if (Directions.Contains("down")) nextPosition.Y += movementSpeed;
                if (Directions.Contains("left")) nextPosition.X -= movementSpeed;
                if (Directions.Contains("right")) nextPosition.X += movementSpeed;

                foreach (string direction in Directions)
                    if (!GetCollision(direction, nextPosition))
                        Position = nextPosition;

                Facing = primaryDirection;
                SetAnimation($"walk_{Facing}");
            }

        }
        #endregion

        public void Update(GameTime gameTime, KeyHandler keyHandler)
        {

            // if any of the movement keys are held, move - otherwise skip entirely
            if (Enumerable.Any(new List<bool>
            {
                keyHandler.Get(Keys.W),
                keyHandler.Get(Keys.S),
                keyHandler.Get(Keys.A),
                keyHandler.Get(Keys.D),
            }))
                Move(gameTime, keyHandler);


            #region Handle Animation

            // if time spent on frame has reached max frame length
            if (FrameTime >= FrameLength)
            { FrameIndex += 1; FrameTime = 0; }

            // if last frame of animation loop has been reached
            if (FrameIndex > CurrentAnimation.Item1.Length - 1)
                FrameIndex = 0;

            // set frame to current item in list
            Frame = CurrentAnimation.Item1[FrameIndex];
            #endregion


            #region Handle Rectangles

            // frame source is portion of sprite sheet to grab
            FrameSource = new Rectangle(
                (int)GetSprite(Frame).X,
                (int)GetSprite(Frame).Y,
                (int)Size.X,
                (int)Size.Y);

            // frame output is final displayed sprite on screen
            FrameOutput = new Rectangle(
                (int)Rectangle.X,
                (int)Rectangle.Y,
                Rectangle.Width,
                Rectangle.Height);

            // hitbox follows sprite and causes collisions
            HitBox = new Rectangle(
                FrameOutput.X
                    - (IsCentered ? 
                        (int)HitBoxSize.X / 2
                        : - Rectangle.Width / 2
                          + (int)HitBoxSize.X / 2),
                FrameOutput.Y
                    + Rectangle.Height / 2
                    - (int)HitBoxSize.Y / (IsCentered ? 1 : 2),

                (int)HitBoxSize.X,
                (int)HitBoxSize.Y);
            #endregion


            // increment frame time for animation
            FrameTime += gameTime.ElapsedGameTime.TotalSeconds;
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                SpriteSheet,
                sourceRectangle: FrameSource,
                destinationRectangle: FrameOutput,
                origin: IsCentered ? Size / 2 : new Vector2(0));
        }
    }
    #endregion
}
