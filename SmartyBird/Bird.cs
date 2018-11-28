using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeuralNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartyBird
{
    class Bird
    {
        public Vector2 Position;
        public Vector2 Speed;

        public Rectangle Hitbox => new Rectangle(Position.ToPoint(),
            new Point((int)(texture.Width * scale), (int)(texture.Height * scale)));

        public int Fitness = 0;
        public bool IsAlive = true;

        public NeuralNet Brain;
        public void Crossover(NeuralNet other, Random rng) => Brain.Crossover(other, rng);
        public void Mutate(Random rng, double rate) => Brain.Mutate(rng, rate);
        public void Randomize(Random rng) => Brain.Randomize(rng);

        private Texture2D texture;
        private float scale = 0.05f;
        
        public Bird(Texture2D texture, Vector2 position, NeuralNet brain)
        {
            this.texture = texture;
            Position = position;

            Brain = brain;

            Speed = Vector2.Zero;
        }

        public void Update()
        {
            if (!IsAlive) return;

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                Speed.Y = -10;
            }
            Position += Speed;
            Speed.Y += 0.5f;
        }
        public void Update(List<PipePair> pipes)
        {
            Update(pipes.ToArray());
        }
        public void Update(PipePair[] pipes)
        {
            if (!IsAlive) return;

            var closestPipe = pipes.OrderBy(pipe => pipe.X)
                                    .Where(pipe => pipe.X > Position.X - 50)
                                    .First();

            double[] inputs = new double[3];
            inputs[0] = (double)closestPipe.X / ScreenSize.Width;
            inputs[1] = 0.5d + 0.5 * ((closestPipe.Height - Position.Y) / ScreenSize.Width);
            inputs[2] = (double)Position.Y / ScreenSize.Height;

            if (Brain.Compute(inputs)[0] == 1)
            {
                Speed.Y = -10;
            }
            else
            {
                ;
            }
            Fitness++;

            Position += Speed;
            Speed.Y += 0.5f;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(texture, Position, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }
}
