// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ToggleGraph : Drawable
    {
        private float timeUntilEnd = 3000;

        public float TimeUntilEnd
        {
            get => timeUntilEnd;
            set
            {
                if (timeUntilEnd == value)
                    return;

                timeUntilEnd = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private BarDirection direction = BarDirection.BottomToTop;

        public BarDirection Direction
        {
            get => direction;
            set
            {
                if (direction == value)
                    return;

                direction = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private bool startState;
        private readonly LimitedCapacityQueue<float> timeStamps = new LimitedCapacityQueue<float>(100);

        private IShader shader = null!;
        private Texture texture = null!;

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer, ShaderManager shaders)
        {
            texture = renderer.WhitePixel;
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
        }

        public void SetState(bool activated)
        {
            if (activated == startState)
                return;

            bool isFull = timeStamps.Full;

            timeStamps.Enqueue((float)Time.Current);

            if (isFull && activated)
                timeStamps.Dequeue();

            startState = activated;
        }

        protected override DrawNode CreateDrawNode() => new ToggleGraphDrawNode(this);

        private class ToggleGraphDrawNode : DrawNode
        {
            public new ToggleGraph Source => (ToggleGraph)base.Source;

            public ToggleGraphDrawNode(ToggleGraph source)
                : base(source)
            {
            }

            private IShader shader = null!;
            private Texture texture = null!;

            private Vector2 drawSize;
            private BarDirection direction;
            private double timeUntilEnd;

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                drawSize = Source.DrawSize;
                direction = Source.direction;
                timeUntilEnd = Source.timeUntilEnd;
            }

            public override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                float offset = (float)Source.Time.Current;

                float end = 1;

                while (Source.timeStamps.Count > 0 && (offset - Source.timeStamps[0]) / timeUntilEnd >= 1)
                    Source.timeStamps.Dequeue();

                bool state = Source.startState ^ (Source.timeStamps.Count % 2 == 1);

                shader.Bind();

                foreach (float timeStamp in Source.timeStamps.AsEnumerable())
                {
                    float start = (float)((offset - timeStamp) / timeUntilEnd);

                    state = !state;

                    if (state)
                    {
                        end = start;

                        continue;
                    }

                    drawBar(start);
                }

                if (Source.startState)
                    drawBar(0);

                void drawBar(float start)
                {
                    if (start >= end)
                        return;

                    float width = direction <= BarDirection.RightToLeft ? end - start : 1;
                    float height = direction >= BarDirection.TopToBottom ? end - start : 1;

                    Vector2 topLeft;

                    switch (direction)
                    {
                        default:
                        case BarDirection.LeftToRight:
                            topLeft = new Vector2(start, 0);
                            break;

                        case BarDirection.RightToLeft:
                            topLeft = new Vector2(1 - start - width, 0);
                            break;

                        case BarDirection.TopToBottom:
                            topLeft = new Vector2(0, start);
                            break;

                        case BarDirection.BottomToTop:
                            topLeft = new Vector2(0, 1 - start - height);
                            break;
                    }

                    RectangleF rectangle = new RectangleF(topLeft, new Vector2(width, height));

                    // var polygon = createPolygon(quad);

                    // renderer.DrawClipped(ref polygon, texture, getColourInfo(rectangle, direction));

                    renderer.DrawQuad(
                        texture,
                        new Quad(
                            rectangle.X * drawSize.X,
                            rectangle.Y * drawSize.Y,
                            rectangle.Width * drawSize.X,
                            rectangle.Height * drawSize.Y
                        ) * DrawInfo.Matrix,
                        getColourInfo(rectangle, direction)
                    );
                }

                shader.Unbind();
            }

            private SimpleConvexPolygon createPolygon(Quad quad, int verticesCount = 20)
            {
                Vector2[] vertices = new Vector2[verticesCount];

                float radiusW = quad.Width / 2;
                float radiusH = quad.Height / 2;

                Vector2 center = quad.TopLeft + new Vector2(radiusW, radiusH);

                float step = 2 * MathHelper.Pi / (verticesCount - 1);

                for (int i = 0; i < verticesCount; i++)
                {
                    vertices[i] = center + new Vector2(MathF.Cos(i * step) * radiusW, MathF.Sin(i * step) * radiusH);
                }

                return new SimpleConvexPolygon(vertices);
            }

            private ColourInfo getColourInfo(RectangleF rectangle, BarDirection direction)
            {
                float top = direction == BarDirection.BottomToTop ? rectangle.Top : direction == BarDirection.TopToBottom ? 1 - rectangle.Top : 0;
                float bottom = direction == BarDirection.BottomToTop ? rectangle.Bottom : direction == BarDirection.TopToBottom ? 1 - rectangle.Bottom : 0;
                float left = direction == BarDirection.RightToLeft ? rectangle.Left : direction == BarDirection.LeftToRight ? 1 - rectangle.Left : 0;
                float right = direction == BarDirection.RightToLeft ? rectangle.Right : direction == BarDirection.LeftToRight ? 1 - rectangle.Right : 0;

                ColourInfo c = new ColourInfo
                {
                    TopLeft = DrawColourInfo.Colour.Interpolate(rectangle.TopLeft),
                    TopRight = DrawColourInfo.Colour.Interpolate(rectangle.TopRight),
                    BottomLeft = DrawColourInfo.Colour.Interpolate(rectangle.BottomLeft),
                    BottomRight = DrawColourInfo.Colour.Interpolate(rectangle.BottomRight)
                };

                c.TopLeft.MultiplyAlpha(top + left);
                c.TopRight.MultiplyAlpha(top + right);
                c.BottomLeft.MultiplyAlpha(bottom + left);
                c.BottomRight.MultiplyAlpha(bottom + right);

                return c;
            }
        }
    }
}
