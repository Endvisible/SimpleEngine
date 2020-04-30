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

        protected int Resize;   // size multiplier

        protected bool IsCentered;       // tile origin is center
        protected int[] CollisionBounds; // hitbox widths (inset)

        public Vector2 Position;         // position on screen
        public bool IsBoundary;          // tile has hitbox

        public Vector2 Size; // size in pixels
        
        public Rectangle FrameSource; // sourceRectangle
        public Rectangle FrameOutput; // destinationRectangle

        public Dictionary<string, Rectangle> Boundaries;

        public Rectangle Rectangle
        { get {
            return new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)Size.X * Resize,
                (int)Size.Y * Resize);
        } }

        public Vector2 Center
        { get {
            return new Vector2(
                Rectangle.Width / 2,
                Rectangle.Height / 2);
        } }
        #endregion

        public Tile(
            Texture2D SpriteSheet,
            Vector2 Position,
            Vector2 Size,

            int Resize = 1,
            int Frame = 0,
            int ColumnCount = 1,
            int[] CollisionBounds = null,
            bool IsCentered = false,
            bool IsBoundary = false
            )
        {
            this.SpriteSheet = SpriteSheet;
            this.Size = Size;
            this.Position = Position;

            this.Resize = Resize;
            this.ColumnCount = ColumnCount;
            this.Frame = Frame;

            this.IsCentered = IsCentered;
            this.IsBoundary = IsBoundary;

            if (!(CollisionBounds is null)) 
                this.CollisionBounds = 
                    (from item in CollisionBounds select item * this.Resize).ToArray<int>();
            else this.CollisionBounds = new int[] { 0, 0, 0, 0 };

            if (this.IsBoundary) Boundaries = SetBoundaries(this.CollisionBounds);
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

        // set boundaries around tile
        private Dictionary<string, Rectangle> SetBoundaries(int[] collisionBounds)
        {
            if (!IsBoundary) return new Dictionary<string, Rectangle> { };

            Dictionary<string, Rectangle> Boundaries = new Dictionary<string, Rectangle> { };

            // Add top boundary inset
            if (collisionBounds[0] != 0)
                Boundaries.Add("top",
                    new Rectangle(
                        Rectangle.X - (IsCentered ? Rectangle.Width / 2 : 0),
                        Rectangle.Y - (IsCentered ? Rectangle.Height / 2 : 0),
                        Rectangle.Width,
                        collisionBounds[0]));

            // Add right boundary inset
            if (collisionBounds[1] != 0)
                Boundaries.Add("right",
                    new Rectangle(
                        Rectangle.X
                            + Rectangle.Width - collisionBounds[1]
                            - (IsCentered ? Rectangle.Width / 2 : 0),
                        Rectangle.Y - (IsCentered ? Rectangle.Height / 2 : 0),
                        collisionBounds[1],
                        Rectangle.Height));

            // Add bottom boundary inset
            if (collisionBounds[2] != 0)
                Boundaries.Add("bottom",
                    new Rectangle(
                        Rectangle.X - (IsCentered ? Rectangle.Width / 2 : 0),
                        Rectangle.Y
                            + Rectangle.Height - collisionBounds[2]
                            - (IsCentered ? Rectangle.Height / 2 : 0),
                        Rectangle.Width,
                        collisionBounds[2]));

            // Add left boundary inset
            if (collisionBounds[3] != 0)
                Boundaries.Add("left",
                    new Rectangle(
                        Rectangle.X - (IsCentered ? Rectangle.Width / 2 : 0),
                        Rectangle.Y - (IsCentered ? Rectangle.Height / 2 : 0),
                        collisionBounds[3],
                        Rectangle.Height));

            return Boundaries;
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

            Boundaries = SetBoundaries(CollisionBounds);
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

        private float SpeedMultiplier; // speed multiplier

        private string Facing;    // direction sprite is facing

        private Vector2 HitBoxSize; // size of hitbox

        private int FrameIndex = 0;            // animation frame counter
        private double FrameTime = 0;          // frame time counter
        private double FrameLength = 1f / 30f; // frame rate

        private readonly List<string> Directions;   // keys held (in order)
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
            
            int Resize = 1,
            bool IsCentered = false,
            bool IsBoundary = false,
            int ColumnCount = 1,
            Vector2 HitBoxSize = new Vector2(),

            int Speed = 60) 

            : base(SpriteSheet, Position, Size)
        {
            this.Position = Position;
            this.Size = Size;

            this.Resize = Resize;
            this.IsCentered = IsCentered;
            this.IsBoundary = IsBoundary;
            this.ColumnCount = ColumnCount;
            this.HitBoxSize = HitBoxSize;

            this.Speed = Speed * Resize;

            SpeedMultiplier = 1;

            Facing = "down";
            Directions = new List<string>(capacity: 4);

            if (!(HitBoxSize == default(Vector2)))
                this.HitBoxSize = SetHitBox(HitBoxSize);
            else this.HitBoxSize = SetHitBox(this.Size);

            CurrentAnimation = Animations["default"];
            Frame = 0;
        }

        #region Functions
        private Vector2 SetHitBox(Vector2 size)
        {
            return new Vector2(
                HitBoxSize.X = size.X * Resize,
                HitBoxSize.Y = size.Y * Resize);
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

        public bool GetCollision(string direction)
        {
            return true;
        }

        public void Move(GameTime gameTime, KeyHandler keyHandler)
        {
            // speed is multiplied if shift is held, then reset - otherwise, skip entirely
            if (keyHandler.Get(Keys.LeftShift)) SpeedMultiplier = 2;
            else if (SpeedMultiplier != 1) SpeedMultiplier = 1;

            // list of direction keys being held
            List<string> Directions = keyHandler.Directions;

            // first direction held gets priority
            string primaryDirection;
            if (keyHandler.Directions.Count > 0)
                primaryDirection = keyHandler.Directions[0];
            else primaryDirection = Facing;


            float movementSpeed = 
                Speed
                * (float)gameTime.ElapsedGameTime.TotalSeconds
                * SpeedMultiplier;

            Vector2 nextPosition = Position;

            // if sprite is not moving, set animation to idle facing
            if (keyHandler.Directions.Count == 0) SetAnimation($"idle_{Facing}");

            // if sprite is moving, move and animate along facing
            else
            {

                //if (Directions.Contains("up"))    nextPosition.Y -= movementSpeed;
                //if (Directions.Contains("down"))  nextPosition.Y += movementSpeed;
                //if (Directions.Contains("left"))  nextPosition.X -= movementSpeed;
                //if (Directions.Contains("right")) nextPosition.X += movementSpeed;

                if (Directions.Contains("up")) Position.Y -= movementSpeed;
                if (Directions.Contains("down")) Position.Y += movementSpeed;
                if (Directions.Contains("left")) Position.X -= movementSpeed;
                if (Directions.Contains("right")) Position.X += movementSpeed;

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

            Console.WriteLine(Position);


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
