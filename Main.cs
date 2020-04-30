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

        Scene livingRoom;
        Sprite player;

        int BaseResize = 4;
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


            livingRoom = new Scene(
                SpriteSheet: Content.Load<Texture2D>("tileset-indoors"),
                Size: new Vector2(32, 32),

                Layout: new float[][] {
                    new float[]{ 6.3003f, 5.3000f, 5.3000f, 4.3300f },
                    new float[]{ 7.0003f, 0,       0,       3.0300f },
                    new float[]{ 7.0003f, 0,       0,       3.0300f },
                    new float[]{ 8.0033f, 1.0030f, 1.0030f, 2.0330f }},

                IsCentered: true,
                Resize: BaseResize,
                ColumnCount: 9
            );

            livingRoom.Offset(new Vector2(
                ScreenWidth / 2,
                ScreenHeight / 2));
            
            CurrentScene = livingRoom;

            player = new Sprite(
                SpriteSheet: Content.Load<Texture2D>("char"),
                Position: CurrentScene.Center,
                Size: new Vector2(32),

                Resize: BaseResize,
                IsCentered: true,
                ColumnCount: 16,

                Speed: 20,
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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            livingRoom.Draw(spriteBatch);
            DrawBoundaries(spriteBatch, livingRoom);
            player.Draw(spriteBatch);

            DrawCenter(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawBoundaries(SpriteBatch spriteBatch, Scene scene)
        {
            foreach(Tile tile in scene.Tiles)
                if (tile.IsBoundary)
                    foreach (Rectangle boundary in tile.Boundaries.Values)
                        spriteBatch.Draw(_debugTexture, boundary, Color.White);

            spriteBatch.Draw(_debugTexture, player.HitBox, Color.White);
        }

        public void DrawCenter(SpriteBatch spriteBatch) {
            for (int i = 0; i < ScreenWidth; i += player.Rectangle.Width)
                spriteBatch.Draw(_debugTexture, new Rectangle(i, 0, 1, ScreenHeight), new Color(128, 128, 128, 128));
            for (int i = -6*BaseResize; i < ScreenHeight; i += player.Rectangle.Height)
                spriteBatch.Draw(_debugTexture, new Rectangle(0, i, ScreenWidth, 1), new Color(128, 128, 128, 128));
            spriteBatch.Draw(_debugTexture, new Rectangle(ScreenWidth / 2, 0, 1, ScreenHeight), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(0, ScreenHeight / 2, ScreenWidth, 1), Color.White);
        }
    }
}
