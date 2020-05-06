using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace SimpleEngine
{
    public class Main : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyHandler keyHandler;
        Texture2D _debugTexture;

        Scene CurrentScene;
        Camera Camera;

        Scene livingRoom;
        Sprite player;

        int ScreenWidth { get { return Window.ClientBounds.Width; } }
        int ScreenHeight { get { return Window.ClientBounds.Height; } }

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            keyHandler = new KeyHandler();
            Camera = new Camera(GraphicsDevice.Viewport) { Zoom = 3 };


            livingRoom = new Scene(
                SpriteSheet: Content.Load<Texture2D>("tileset-indoors"),
                Size: new Vector2(32, 32),

                Layout: new float[][] {
                    new float[]{ 6, 5, 5, 4 },
                    new float[]{ 7, 0, 0, 3 },
                    new float[]{ 7, 0, 0, 3 },
                    new float[]{ 8, 1, 1, 2 }},

                IsCentered: false,
                ColumnCount: 9
            );
            
            CurrentScene = livingRoom;
            CurrentScene.AddBoundary(new Rectangle(0, 0, CurrentScene.Width, 3));
            CurrentScene.AddBoundary(new Rectangle(CurrentScene.Width-3, 3, 3, CurrentScene.Height-3));
            CurrentScene.AddBoundary(new Rectangle(0, CurrentScene.Height-3, CurrentScene.Width, 3));
            CurrentScene.AddBoundary(new Rectangle(0, 3, 3, CurrentScene.Height - 3));


            player = new Sprite(
                SpriteSheet: Content.Load<Texture2D>("char"),
                Position: CurrentScene.Center,
                Size: new Vector2(32),

                IsCentered: true,
                ColumnCount: 16,

                Speed: 40,
                HitBoxSize: new Vector2(24, 31)
            );

            player.SetScene(CurrentScene, "player");

            #region IDLE ANIMATIONS
            player.AddAnimation(
                "idle_down",
                new Tuple<int[], int>(new int[] { 0, 1, }, 2)
                );
            player.AddAnimation(
                "idle_up",
                new Tuple<int[], int>(new int[] { 2, 3, }, 2)
                );
            player.AddAnimation(
                "idle_left",
                new Tuple<int[], int>(new int[] { 4, 5, }, 2)
                );
            player.AddAnimation(
                "idle_right",
                new Tuple<int[], int>(new int[] { 6, 7, }, 2)
                );
            #endregion

            #region WALK ANIMATIONS
            player.AddAnimation(
                "walk_down",
                new Tuple<int[], int>(new int[] { 8, 9, 10, 9, 8, 11, 12, 11, }, 8) 
                );
            player.AddAnimation(
                "walk_up",
                new Tuple<int[], int>(new int[] { 13, 14, 15, 14, 13, 16, 17, 16, }, 8)
                );
            player.AddAnimation(
                "walk_left",
                new Tuple<int[], int>(new int[] { 18, 19, 20, 19, 18, 21, 22, 21, }, 8)
                );
            player.AddAnimation(
                "walk_right",
                new Tuple<int[], int>(new int[] { 23, 24, 25, 24, 23, 26, 27, 26, }, 8)
                );
            #endregion

            player.SetAnimation("walk_down");

            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new Color[] { Color.Green });
        }

        protected override void UnloadContent()
        {
            player.SpriteSheet.Dispose();
            CurrentScene.Dispose();
        }


        protected override void Update(GameTime gameTime)
        {
            if (!IsActive) return; // Do not update game if it is not active

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed 
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            keyHandler.Update();

            player.Update(gameTime, keyHandler);

            Camera.Update(player);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: Camera.Transform);

            livingRoom.Draw(spriteBatch);
            DrawBoundaries(spriteBatch);
            player.Draw(spriteBatch);

            //DrawCenter(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawBoundaries(SpriteBatch spriteBatch)
        {
            foreach (Rectangle boundary in CurrentScene.Boundaries)
                spriteBatch.Draw(_debugTexture, boundary, Color.White);
            spriteBatch.Draw(_debugTexture, player.HitBox, Color.White);
        }

        public void DrawCenter(SpriteBatch spriteBatch) {
            for (int i = 0; i < ScreenWidth; i += player.Rectangle.Width)
                spriteBatch.Draw(_debugTexture, new Rectangle(i, 0, 1, ScreenHeight), new Color(128, 128, 128, 128));
            for (int i = -6; i < ScreenHeight; i += player.Rectangle.Height)
                spriteBatch.Draw(_debugTexture, new Rectangle(0, i, ScreenWidth, 1), new Color(128, 128, 128, 128));
            spriteBatch.Draw(_debugTexture, new Rectangle(ScreenWidth / 2, 0, 1, ScreenHeight), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(0, ScreenHeight / 2, ScreenWidth, 1), Color.White);
        }
    }
}
