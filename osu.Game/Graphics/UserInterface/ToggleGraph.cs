// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

                //float[] timestamps = Source.timeStamps.AsEnumerable().ToArray();

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

                    float width = (direction <= BarDirection.RightToLeft ? end - start : 1);
                    float barWidth = drawSize.X * width;
                    float height = (direction >= BarDirection.TopToBottom ? end - start : 1);
                    float barHeight = drawSize.Y * height;

                    Vector2 topLeft;

                    switch (direction)
                    {
                        default:
                        case BarDirection.LeftToRight:
                            topLeft = new Vector2(1 - start - width, 0);
                            break;

                        case BarDirection.RightToLeft:
                            topLeft = new Vector2(start, 0);
                            break;

                        case BarDirection.TopToBottom:
                            topLeft = new Vector2(0, start);
                            break;

                        case BarDirection.BottomToTop:
                            topLeft = new Vector2(0, 1 - start - height);
                            break;
                    }

                    Vector2 barTopLeft = new Vector2(topLeft.X * drawSize.X, topLeft.Y * drawSize.Y);

                    Quad quad = new Quad(
                        Vector2Extensions.Transform(barTopLeft, DrawInfo.Matrix),
                        Vector2Extensions.Transform(barTopLeft + new Vector2(barWidth, 0), DrawInfo.Matrix),
                        Vector2Extensions.Transform(barTopLeft + new Vector2(0, barHeight), DrawInfo.Matrix),
                        Vector2Extensions.Transform(barTopLeft + new Vector2(barWidth, barHeight), DrawInfo.Matrix)
                    );
                    renderer.DrawQuad(
                        texture,
                        quad,
                        new ColourInfo
                        {
                            TopLeft = DrawColourInfo.Colour.Interpolate(topLeft),
                            TopRight = DrawColourInfo.Colour.Interpolate(topLeft + new Vector2(width, 0)),
                            BottomLeft = DrawColourInfo.Colour.Interpolate(topLeft + new Vector2(0, height)),
                            BottomRight = DrawColourInfo.Colour.Interpolate(topLeft + new Vector2(width, height))
                        });
                }

                shader.Unbind();
            }
        }
    }
}
