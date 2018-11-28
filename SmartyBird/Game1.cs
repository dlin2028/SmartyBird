using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeuralNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartyBird
{
    public class Game1 : Game
    {
        readonly GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Bird userBird;
        Bird smartyBird;
        List<PipePair> pipes;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1000;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 700;   // set this value to the desired height of your window
            graphics.ApplyChanges();

            ScreenSize.Width = GraphicsDevice.Viewport.Width;
            ScreenSize.Height = GraphicsDevice.Viewport.Height;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            var pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            pipes = new List<PipePair>();
            pipes.Add(new PipePair(pixel, 300, 50, 200, 100, 5));
            pipes.Add(new PipePair(pixel, 600, 50, 200, 100, 5));
            pipes.Add(new PipePair(pixel, 900, 50, 200, 100, 5));

            userBird = new Bird(Content.Load<Texture2D>("bird"), new Vector2(50, 150), null);
            smartyBird = TrainBird(Content.Load<Texture2D>("bird"), 100, 500, 10000);
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            foreach (var pipe in pipes)
            {
                if (pipe.Intersects(userBird.Hitbox) || userBird.Position.Y > ScreenSize.Height || userBird.Position.Y < 0)
                {
                    userBird.IsAlive = false;
                }
                if (pipe.Intersects(smartyBird.Hitbox) || smartyBird.Position.Y > ScreenSize.Height || smartyBird.Position.Y < 0)
                {
                    smartyBird.IsAlive = false;
                }

                pipe.Update();
            }
            userBird.Update();

            smartyBird.Update(pipes);

            if(Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                smartyBird.Position = new Vector2(50, 150);
                smartyBird.IsAlive = true;
                smartyBird.Speed = Vector2.Zero;

                userBird.Position = new Vector2(50, 150);
                userBird.IsAlive = true;
                userBird.Speed = Vector2.Zero;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            userBird.Draw(spriteBatch);
            smartyBird.Draw(spriteBatch);
            foreach (var pipe in pipes)
            {
                pipe.Draw(spriteBatch);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        Bird TrainBird(Texture2D texture, int birdCount, int maxGenerations, int maxScore)
        {
            List<Bird> birds = new List<Bird>();
            Random rng = new Random();

            for (int i = 0; i < birdCount; i++)
            {
                birds.Add(new Bird(texture, new Vector2(50, 150), new NeuralNet(Activations.BinaryStep, 3, 10, 15, 10, 1)));
                birds[i].Randomize(rng);
            }

            int gen = 0;
            while (true)
            {
                while (birds.Where(brd => brd.IsAlive).FirstOrDefault() != null)
                {
                    
                    foreach (var brd in birds)
                    {
                        foreach (var pipe in pipes)
                        {
                            if (pipe.Intersects(brd.Hitbox) || brd.Position.Y > ScreenSize.Height || brd.Position.Y < 0)
                            {
                                brd.IsAlive = false;
                            }
                            else if(brd.Fitness == maxScore)
                            {
                                brd.Position = new Vector2(50, 150);
                                brd.IsAlive = true;
                                brd.Speed = Vector2.Zero;
                                brd.Fitness = 0;
                                return brd;
                            }
                        }
                        brd.Update(pipes);
                    }
                    /*
                    Parallel.ForEach(birds, (brd) =>
                    {
                        foreach (var pipe in pipes)
                        {
                            if (pipe.Intersects(brd.Hitbox) || brd.Position.Y > ScreenSize.Height || brd.Position.Y < 0)
                            {
                                brd.IsAlive = false;
                            }
                        }
                        brd.Update(pipes);
                    });*/


                    foreach (var pipe in pipes)
                    {
                        pipe.Update();
                    }
                }


                birds.Sort((y, x) => x.Fitness.CompareTo(y.Fitness));
                

                foreach (var bird in birds)
                {
                    bird.Position = new Vector2(50, 150);
                    bird.IsAlive = true;
                    bird.Speed = Vector2.Zero;
                    bird.Fitness = 0;
                }
                for (int i = 0; i < pipes.Count; i++)
                {
                    pipes[i].X = 300 * i;
                }

                if (gen == maxGenerations)
                {
                    break;
                }

                int start = (int)(birds.Count * 0.05);
                int end = (int)(birds.Count * 0.90);

                for (int i = start; i < end; i++)
                {
                    birds[i].Crossover(birds[rng.Next(start)].Brain, rng);
                    birds[i].Mutate(rng, 0.2);
                }

                for (int i = end; i < birds.Count; i++)
                {
                    birds[i].Randomize(rng);
                }

                gen++;
            }


            return birds[0];
        }
    }
}
