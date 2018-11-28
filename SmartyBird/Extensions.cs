using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartyBird
{
    public static class Extensions
    {
        public static Point ToPoint(this Vector2 sender)
        {
            return new Point((int)sender.X, (int)sender.Y);
        }
    }
}
