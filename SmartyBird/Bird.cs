using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private Texture2D texture;
        public float Scale = 0.1f;

        public Bird(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            Position = position;
            Speed = Vector2.Zero;
        }

        public void Update()
        {
            Position += Speed;
            Speed.Y++;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(texture, Position, null, Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }
    }
}
