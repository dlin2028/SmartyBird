using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartyBird
{
    class Pipes
    {
        Rectangle top;
        Rectangle bottom;
        Texture2D pixel;

        public Pipes(Texture2D pixel)
        {
            this.pixel = pixel;
            top = new Rectangle(0, 0, 50, 0);
            bottom = new Rectangle(0, 0, 50, 0);
        }

        public void Reset(Vector2 screenSize)
        {
            Random rng = new Random();
            top.X = (int)screenSize.X - 50;
            bottom.X = (int)screenSize.X - 50;
            top.Height = rng.Next(50, 100);
            bottom.Y = top.Height + 50;
        }

        public void Update(Vector2 screenSize)
        {
            top.X--;
            bottom.X--;

            if(top.X < -50)
            {
                Reset(screenSize);
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixel, top, Color.White);
            spriteBatch.Draw(pixel, bottom, Color.White);
        }
    }
}
