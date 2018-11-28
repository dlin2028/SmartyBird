using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartyBird
{
    class PipePair
    {
        public int X
        {
            get
            {
                return top.X;
            }
            set
            {
                top.X = value;
            }
        }
        public int Height => bottom.Y;

        public bool Intersects(Rectangle other) => top.Intersects(other) || bottom.Intersects(other);

        private Rectangle top;
        private Rectangle bottom => new Rectangle(top.X, top.Height + gapSize, top.Width, ScreenSize.Height);

        private int gapSize;
        private int gapBuffer;
        
        private Texture2D pixel;
        private int speed;

        private Random rng = new Random();

        public PipePair(Texture2D pixel, int offset, int width, int gapSize, int gapBuffer,int speed = 1)
        {
            this.pixel = pixel;

            top.X = offset;
            top.Height = rng.Next(gapBuffer, ScreenSize.Height - gapBuffer - gapSize);
            top.Width = width;

            this.speed = -speed;

            this.gapSize = gapSize;
            this.gapBuffer = gapBuffer;
        }

        public void Update()
        {
            top.X += speed;

            if(X < -top.Width)
            {
                top.Height = rng.Next(gapBuffer, ScreenSize.Height - gapBuffer - gapSize);
                top.X = ScreenSize.Width;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixel, top, Color.White);
            spriteBatch.Draw(pixel, bottom, Color.White);
        }
    }
}
