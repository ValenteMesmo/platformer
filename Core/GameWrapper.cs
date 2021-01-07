﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using MonogameGame = Microsoft.Xna.Framework.Game;

namespace Platformer.Desktop
{
    public class GameWrapper : MonogameGame
    {
        GraphicsDeviceManager graphics = null;
        SpriteBatch worldBatch = null;
        SpriteBatch guiBatch = null;

        private Texture2D pixel = null;

        private readonly Game Parent;
        GameObject currentObject = null;
        Collider currentCollider = null;
        int i;
        int j;
        int k;
        int l;
        private DateTime previousUpdate;
        private DateTime currentUpdate;
        private DateTime actualCurrentUpdate;
        private double delta;
        public const double frameRate = 1.0 / 60.0;
        private double accumulator;

        public GameWrapper(Game Parent)
        {
            this.Parent = Parent;

            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            if (false)
            {
                graphics.IsFullScreen = true;
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.ApplyChanges();
            }
            Window.Title = "Platformer";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.ApplyChanges();
            InactiveSleepTime = new TimeSpan(0);
            previousUpdate = DateTime.Now;
        }

        protected override void LoadContent()
        {
            worldBatch = new SpriteBatch(GraphicsDevice);
            guiBatch = new SpriteBatch(GraphicsDevice);
            Parent.LoadContent(Content);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            previousUpdate = currentUpdate;
            currentUpdate = DateTime.Now;
            delta = (currentUpdate - previousUpdate).TotalSeconds;
            if (delta > 0.27)
                delta = 0.27;

            accumulator += delta;

            if (accumulator >= frameRate)
            {
                Parent.CurrentFramesPerSecond = Parent.CurrentFramesPerSecond * 0.99 + ((1 / ((currentUpdate - actualCurrentUpdate).TotalSeconds))) * 0.01;
                actualCurrentUpdate = currentUpdate;

                while (accumulator >= frameRate)
                {
                    ActualUpdate();
                    accumulator -= frameRate;
                }
            }
            else
            {
                for (i = 0; i < Parent.ActiveObjects.Count; i++)
                {
                    currentObject = Parent.ActiveObjects[i];

                    currentObject.PreviousPosition.X = (int)MathHelper.Lerp(currentObject.PreviousPosition.X, currentObject.Position.X, .5f);
                    currentObject.PreviousPosition.Y = (int)MathHelper.Lerp(currentObject.PreviousPosition.Y, currentObject.Position.Y, .5f);
                }
                //SuppressDraw();
            }

            base.Update(gameTime);
        }

        private void ActualUpdate()
        {
           

            Parent.Player1Inputs.Update(Parent.GuiCamera);


                for (i = 0; i < Parent.ActiveObjects.Count; i++)
            {
                currentObject = Parent.ActiveObjects[i];
                currentObject.UpdateHandler();

                currentObject.PreviousPosition = currentObject.Position;

                currentObject.Position.Y += currentObject.Velocity.Y;

                //if (!currentObject.IsPassive)
                for (j = 0; j < currentObject.Colliders.Count; j++)
                {
                    currentCollider = currentObject.Colliders[j];

                    //currentCollider.BeforeCollisionHandler();
                    for (k = 0; k < Parent.PassiveObjects.Count; k++)
                        for (l = 0; l < Parent.PassiveObjects[k].Colliders.Count; l++)
                            CheckCollisions(
                                CollisionDirection.Vertical
                                , currentCollider
                                , Parent.PassiveObjects[k].Colliders[l]);
                }

                currentObject.Position.X += currentObject.Velocity.X;
                //if (!currentObject.IsPassive)
                for (j = 0; j < currentObject.Colliders.Count; j++)
                {
                    currentCollider = currentObject.Colliders[j];

                    //currentCollider.BeforeCollisionHandler();
                    for (k = 0; k < Parent.PassiveObjects.Count; k++)
                        for (l = 0; l < Parent.PassiveObjects[k].Colliders.Count; l++)
                            CheckCollisions(
                                CollisionDirection.Horizontal
                                , currentCollider
                                , Parent.PassiveObjects[k].Colliders[l]);
                }
            }

            for (i = 0; i < Parent.GuiObjects.Count; i++)
            {
                currentObject = Parent.GuiObjects[i];
                currentObject.UpdateHandler();
            }
        }

        private void CheckCollisions(CollisionDirection direction, Collider source, Collider target)
        {
            //var targets = quadtree.Get(source);

            //for (int i = 0; i < targets.Length; i++)
            {
#if DEBUG
                if (source.Parent == null || target.Parent == null)
                    throw new Exception("Collider parent cannot be null!");
#endif

                if (source.Parent == target.Parent)
                    return;

                if (direction == CollisionDirection.Vertical)
                    source.IsCollidingV(target);
                else
                    source.IsCollidingH(target);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            worldBatch.Begin(
                SpriteSortMode.Deferred
                , BlendState.NonPremultiplied
                , SamplerState.LinearClamp
                , DepthStencilState.None
                , RasterizerState.CullNone
                , null
                , Parent.WorldCamera.GetTransformation(GraphicsDevice)
            );
            guiBatch.Begin(
                SpriteSortMode.Deferred
                , BlendState.NonPremultiplied
                , SamplerState.LinearClamp
                , DepthStencilState.None
                , RasterizerState.CullNone
                , null
                , Parent.GuiCamera.GetTransformation(GraphicsDevice)
            );

            for (i = 0; i < Parent.PassiveObjects.Count; i++)
            {
                Parent.PassiveObjects[i].RenderHandler.Draw(worldBatch, Parent.PassiveObjects[i]);

                if (Parent.Player1Inputs.ColliderToggle.IsToogled)
                    for (j = 0; j < Parent.PassiveObjects[i].Colliders.Count; j++)
                        DrawBorder(Parent.PassiveObjects[i].Colliders[j].RelativeArea, 600, Color.Red, worldBatch);

            }

            for (i = 0; i < Parent.ActiveObjects.Count; i++)
            {
                Parent.ActiveObjects[i].RenderHandler.Draw(worldBatch, Parent.ActiveObjects[i]);

                if (Parent.Player1Inputs.ColliderToggle.IsToogled)
                    for (j = 0; j < Parent.ActiveObjects[i].Colliders.Count; j++)
                        DrawBorder(Parent.ActiveObjects[i].Colliders[j].RelativeArea, 600, Color.Red, worldBatch);

            }

            for (i = 0; i < Parent.GuiObjects.Count; i++)
            {
                Parent.GuiObjects[i].RenderHandler.Draw(guiBatch, Parent.GuiObjects[i]);
            }

            
            //DrawBorder(TouchPadController.TouchAreaExtraSize, 6, Color.Green, guiBatch);
            worldBatch.End();
            guiBatch.End();

            base.Draw(gameTime);
        }

        private void DrawBorder(Rectangle rectangleToDraw, int thicknessOfBorder, Color borderColor, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thicknessOfBorder), null, borderColor, 0, Vector2.Zero, SpriteEffects.None, 0);
            spriteBatch.Draw(pixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height), null, borderColor, 0, Vector2.Zero, SpriteEffects.None, 0);
            spriteBatch.Draw(pixel, new Rectangle((rectangleToDraw.X + rectangleToDraw.Width - thicknessOfBorder), rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height), null, borderColor, 0, Vector2.Zero, SpriteEffects.None, 0);
            spriteBatch.Draw(pixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y + rectangleToDraw.Height - thicknessOfBorder, rectangleToDraw.Width, thicknessOfBorder), null, borderColor, 0, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
