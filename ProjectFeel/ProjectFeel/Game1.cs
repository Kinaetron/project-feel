using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ProjectFeel
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Vector2 gameScreenSize;

        Texture2D MeatMan;

        Vector2 vel;
        Vector2 pos;

        const float boostMaxHorizSpeed = 10.0f;
        const float normMaxHorizSpeed = 6.0f;
        const float gravity = 0.4f;

        const float normalAccel = 3.0f;
        const float boostAccel = 10.0f;
        const float turnMul = 1.0f;

        const float initialJumpSpeed = -10.0f;

        GamePadState previousGamePadState;
        GamePadState currentState;

        KeyboardState currentStateKey;
        KeyboardState oldState;


        bool isOnGround;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();

            previousGamePadState = GamePad.GetState(PlayerIndex.One);
            oldState = Keyboard.GetState();

            gameScreenSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            MeatMan = Content.Load<Texture2D>("MeatMan");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        bool isJumpPressed() {

            if (IsConnected() == true) {
                if (currentState.Buttons.A == ButtonState.Pressed)
                    return true;
            }
            else {
                if (currentStateKey.IsKeyDown(Keys.Space))
                    return true;
            }

            return false;
        }

        bool isJumpPrevReleased() {

            if (IsConnected() == true) {
                if (previousGamePadState.Buttons.A == ButtonState.Released)
                    return true;
            }
            else {
                if (oldState.IsKeyUp(Keys.Space))
                    return true;
            }

            return false;
        }

        float maxHorizSpeed()
        {
            if (currentState.Triggers.Right > 0.3f ||
                currentStateKey.IsKeyDown(Keys.Z))
            {
                return boostMaxHorizSpeed;
            }
            return normMaxHorizSpeed;
        }

        void step()
        {
            vel += new Vector2(0, gravity);

            if (vel.X > maxHorizSpeed())
                vel.X = maxHorizSpeed();
            if (vel.X < -maxHorizSpeed())
                vel.X = -maxHorizSpeed();
      
            pos += vel;
        }

        bool IsConnected() {
            if (GamePad.GetState(PlayerIndex.One).IsConnected == true)
                return true;

            return false;
        }

        float walkAccel()
        {
            if (currentState.Triggers.Right > 0.3f || 
                currentStateKey.IsKeyDown(Keys.Z))
            {
                return boostAccel;
            }
            return normalAccel;
        }

        bool walk()
        {
            float sign = 0;

            if (currentState.ThumbSticks.Left.X > 0.4f || 
                currentStateKey.IsKeyDown(Keys.Right))
                sign = 1;
            else if (currentState.ThumbSticks.Left.X < -0.4f || 
                currentStateKey.IsKeyDown(Keys.Left))
                sign = -1;
            else
                return false;

            float currSign = Math.Sign(vel.X);
            float v = walkAccel();

            if (currSign != 0 && currSign != sign)
                v *= turnMul;

            vel.X += v * sign;
            return true;
        }

        void airControls()
        {
            /* Abort jump if user lets go of button */
            if ((vel.Y < 0) && (isJumpPressed() == false))
                vel.Y = 0;

            /* Air walk */
            walk();
        }

        void groundControls()
        {
            /* Start jump from ground */
            if (isJumpPressed() == true && isJumpPrevReleased() == true)
            {
                vel.Y = initialJumpSpeed;
                return;
            }
            else
            {
                vel.Y = 0;
            }

            /* Run on ground */
            if (walk() == false)
                vel.X = 0;
        }

        private void PlayerCollisionWithFloor()
        {
            isOnGround = false;

            if (pos.X <= 0)
                pos = new Vector2(0, pos.Y);
            else if (pos.X >= gameScreenSize.X - 32)
                pos = new Vector2(gameScreenSize.X - 32, pos.Y);

            if (pos.Y <= 30)
            {
                pos = new Vector2(pos.X, 30);
            }
            else if (pos.Y >= gameScreenSize.Y - 30)
            {
                isOnGround = true;
                pos = new Vector2(pos.X, gameScreenSize.Y - 30);
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            currentState = GamePad.GetState(PlayerIndex.One);
            currentStateKey = Keyboard.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape) == true)
                this.Exit();

            if (currentState.IsButtonDown(Buttons.Back) == true)
                this.Exit();

            walk();
 
            if (isOnGround == true) {
                groundControls();
            }

            airControls();
            step();

            PlayerCollisionWithFloor();

            previousGamePadState = currentState;
            oldState = currentStateKey;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.Draw(MeatMan, pos, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}