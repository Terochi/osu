// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Timing;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ToggleGraph : Drawable
    {
        public readonly Bindable<float> GraphTimeSpan = new Bindable<float>(3000);

        private readonly Bindable<bool> startState = new BindableBool();

        /// <summary>
        /// Queue containing time stamps of all pressed state changes
        /// </summary>
        private readonly LimitedCapacityQueue<float> changeTimestamps = new LimitedCapacityQueue<float>(100);

        private IShader shader = null!;
        private Texture texture = null!;

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer, ShaderManager shaders)
        {
            texture = renderer.WhitePixel;
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
        }

        protected override void LoadComplete()
        {
            GraphTimeSpan.BindValueChanged(_ => Invalidate(Invalidation.DrawNode));

            base.LoadComplete();
        }

        public void SetState(bool activated)
        {
            if (activated == startState.Value)
                return;

            bool isFull = changeTimestamps.Full;

            changeTimestamps.Enqueue((float)Time.Current);

            if (isFull && activated)
                changeTimestamps.Dequeue();

            startState.Value = activated;
        }

        protected override DrawNode CreateDrawNode() => new ToggleGraphDrawNode(this);

        private class ToggleGraphDrawNode : DrawNode
        {
            public new ToggleGraph Source => (ToggleGraph)base.Source;

            private readonly IFrameBasedClock clock;

            private readonly LimitedCapacityQueue<float> timestamps;
            private readonly Bindable<bool> pressedState;

            public ToggleGraphDrawNode(ToggleGraph source)
                : base(source)
            {
                clock = source.Clock;
                timestamps = source.changeTimestamps;
                pressedState = source.startState.GetBoundCopy();
            }

            private IShader shader = null!;
            private Texture texture = null!;

            private Vector2 drawSize;
            private float graphTimeSpan;

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                drawSize = Source.DrawSize;
                graphTimeSpan = Source.GraphTimeSpan.Value;
            }

            public override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                float currentTime = (float)clock.CurrentTime;

                float end = 1;

                while (timestamps.Count > 0 && (currentTime - timestamps[0]) / graphTimeSpan >= 1)
                    // Remove all timestamps that are out of range
                    timestamps.Dequeue();

                // The pressed state of last timestamp (because we plan to enumerate from oldest to newest)
                bool state = pressedState.Value ^ (timestamps.Count % 2 == 1);

                shader.Bind();

                foreach (float timeStamp in timestamps.AsEnumerable())
                {
                    float start = (currentTime - timeStamp) / graphTimeSpan;

                    state = !state;

                    if (state)
                    {
                        end = start;

                        continue;
                    }

                    // Draw when we know the quads start and end relative positions
                    drawBar(start);
                }

                if (pressedState.Value)
                    // Draw bottom quad because we don't have the end timestamp
                    drawBar(0);

                void drawBar(float start)
                {
                    if (start >= end)
                        return;

                    RectangleF rectangle = new RectangleF(new Vector2(0, 1 - end), new Vector2(1, end - start));

                    renderer.DrawQuad(
                        texture,
                        new Quad(
                            0, //rectangle.X * drawSize.X
                            rectangle.Y * drawSize.Y,
                            drawSize.X, //rectangle.Width * drawSize.X
                            rectangle.Height * drawSize.Y
                        ) * DrawInfo.Matrix,
                        getColourInfo(rectangle)
                    );
                }

                shader.Unbind();
            }

            private ColourInfo getColourInfo(RectangleF rectangle)
            {
                ColourInfo colourInfo = new ColourInfo
                {
                    TopLeft = DrawColourInfo.Colour.Interpolate(rectangle.TopLeft),
                    TopRight = DrawColourInfo.Colour.Interpolate(rectangle.TopRight),
                    BottomLeft = DrawColourInfo.Colour.Interpolate(rectangle.BottomLeft),
                    BottomRight = DrawColourInfo.Colour.Interpolate(rectangle.BottomRight)
                };

                colourInfo.TopLeft.MultiplyAlpha(rectangle.Top);
                colourInfo.TopRight.MultiplyAlpha(rectangle.Top);
                colourInfo.BottomLeft.MultiplyAlpha(rectangle.Bottom);
                colourInfo.BottomRight.MultiplyAlpha(rectangle.Bottom);

                return colourInfo;
            }
        }
    }
}
