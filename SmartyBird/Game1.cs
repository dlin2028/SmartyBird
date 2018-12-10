using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeuralNetwork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        SpriteFont font;

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

            font = Content.Load<SpriteFont>("font");

            var pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            pipes = new List<PipePair>();
            pipes.Add(new PipePair(pixel, 300, 50, 200, 125, 5));
            pipes.Add(new PipePair(pixel, 600, 50, 200, 125, 5));
            pipes.Add(new PipePair(pixel, 900, 50, 200, 125, 5));

            userBird = new Bird(Content.Load<Texture2D>("bird"), new Vector2(50, 150), null);
            smartyBird = TrainBird(Content.Load<Texture2D>("bird"), 100, 50000, 10000);
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

            if (smartyBird.IsAlive)
            {
                smartyBird.Fitness--;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
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
            spriteBatch.DrawString(font, "Distance to pipe: " + smartyBird.inputs[0].ToString(), Vector2.Zero, Color.Black);
            spriteBatch.DrawString(font, "Distance to gap: " + smartyBird.inputs[1].ToString(), new Vector2(0, 20), Color.Black);
            spriteBatch.DrawString(font, "Y position: " + smartyBird.inputs[2].ToString(), new Vector2(0, 40), Color.Black);
            spriteBatch.DrawString(font, "Bird Fitness: " + smartyBird.Fitness.ToString(), new Vector2(0, 60), Color.Black);


            spriteBatch.End();
            base.Draw(gameTime);
        }

        Bird TrainBird(Texture2D texture, int birdCount, int targetScore, int maxGenerations)
        {
            return TrainBirdParallel(texture, birdCount, targetScore, maxGenerations);
        }

        Bird TrainBirdParallel(Texture2D texture, int birdCount, int targetScore, int maxGenerations)
        {
            Random rng = new Random();
            List<Bird> birds = new List<Bird>();

            for (int i = 0; i < birdCount; i++)
            {
                birds.Add(new Bird(texture, new Vector2(50, 150), new NeuralNet(Activations.BinaryStep, 3, 100, 100, 100, 100, 1)));
                birds[i].Randomize(rng);
            }

            int gen = 0;
            while (true)
            {
                
                bool foundBird = false;
                while (birds.Where(brd => brd.IsAlive).FirstOrDefault() != null)
                {
                    if(foundBird)
                    {
                       break;
                    }
                    
                    Parallel.ForEach(birds, (brd) =>
                    {
                        foreach (var pipe in pipes)
                        {
                            if (pipe.Intersects(brd.Hitbox) || brd.Position.Y > ScreenSize.Height || brd.Position.Y < 0)
                            {
                                brd.IsAlive = false;
                                break;
                            }
                        }
                        brd.Update(pipes);

                        if (brd.Fitness > targetScore)
                        {
                            foundBird = true;
                        }
                    });

                    foreach (var pipe in pipes)
                    {
                        pipe.Update();
                    }


                }

                birds.Sort((y, x) => x.Fitness.CompareTo(y.Fitness));

                if (birds[0].Fitness > targetScore || gen > maxGenerations)
                {
                    return birds[0];
                }

                for (int i = 0; i < pipes.Count; i++)
                {
                    pipes[i].X = 300 * i;
                }

                int start = (int)(birds.Count * 0.05);
                int end = (int)(birds.Count * 0.90);

                Parallel.For(0, birds.Count, (index) =>
                {
                    var bird = birds[index];

                    bird.Position = new Vector2(50, 150);
                    bird.IsAlive = true;
                    bird.Speed = Vector2.Zero;
                    bird.Fitness = 0;

                    if (index > start)
                    {
                        //because threads
                        rng = new Random();
                        if (index < end)
                        {
                            bird.Crossover(birds[rng.Next() % start].Brain, rng);
                            bird.Mutate(rng, 0.2);
                        }
                        else
                        {
                            bird.Randomize(rng);
                        }

                    }
                });

                gen++;
            }
        }

        Bird TrainBirdSequential(Texture2D texture, int birdCount, int targetScore, int maxGenerations)
        {
            List<Bird> birds = new List<Bird>();
            Random rng = new Random();

            for (int i = 0; i < birdCount; i++)
            {
                birds.Add(new Bird(texture, new Vector2(50, 150), new NeuralNet(Activations.BinaryStep, 3, 50, 50, 50, 1)));
                birds[i].Randomize(rng);
            }

            int gen = 0;
            while (true)
            {
                while (birds.Where(brd => brd.IsAlive).FirstOrDefault() != null)
                {
                    /*
                    Bird[] birds2 = new Bird[birds.Count];
                    birds.CopyTo(birds2);

                    PipePair[] pipes2 = new PipePair[pipes.Count];
                    pipes.CopyTo(pipes2);

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    Parallel.ForEach(birds2, (brd) =>
                    {
                        foreach (var pipe in pipes2)
                        {
                            if (pipe.Intersects(brd.Hitbox) || brd.Position.Y > ScreenSize.Height || brd.Position.Y < 0)
                            {
                                brd.IsAlive = false;
                            }
                        }
                        brd.Update(pipes);
                    });
                    stopWatch.Stop();
                    TimeSpan parallel = stopWatch.Elapsed;
                    
                    stopWatch.Restart();
                    */

                    foreach (var brd in birds)
                    {
                        foreach (var pipe in pipes)
                        {
                            if (pipe.Intersects(brd.Hitbox) || brd.Position.Y > ScreenSize.Height || brd.Position.Y < 0)
                            {
                                brd.IsAlive = false;
                            }
                            else if (brd.Fitness == targetScore)
                            {
                                brd.Position = new Vector2(50, 150);
                                brd.IsAlive = true;
                                brd.Speed = Vector2.Zero;
                                return brd;
                            }
                        }
                        brd.Update(pipes);
                    }
                    //stopWatch.Stop();
                    //TimeSpan sequential = stopWatch.Elapsed;

                    foreach (var pipe in pipes)
                    {
                        pipe.Update();
                    }
                }


                birds.Sort((y, x) => x.Fitness.CompareTo(y.Fitness));


                if (gen > maxGenerations)
                {
                    return birds[0];
                }

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
        }
    }
}
