// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    public partial class KeyOverlay : Container
    {
        public readonly Key TargetKey;

        private ToggleGraph graph = null!;
        private OsuSpriteText text = null!;
        private Box box = null!;
        private Color4 color;

        public KeyOverlay(Key targetKey, Color4 innerColor)
        {
            color = innerColor;
            TargetKey = targetKey;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Y;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Size = new Vector2(30, 0.8f);
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(30),
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = 4f,
                    Children = new Drawable[]
                    {
                        box = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(1),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Color4.Transparent
                        },
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Default.With(size: 16, weight: FontWeight.Bold),
                            Colour = Color4.White,
                            Text = //KeyCombination.FromKey(TargetKey).Humanize()
                                TargetKey.Humanize()
                        },
                    }
                },
                graph = new ToggleGraph
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Size = new Vector2(1),
                    Position = new Vector2(0, -30),
                    Direction = BarDirection.BottomToTop,
                    Colour = ColourInfo.GradientVertical(Color4.Transparent, color)
                }
            };
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat && e.Key == TargetKey)
            {
                graph.SetState(true);
                box.Colour = color;
            }

            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == TargetKey)
            {
                graph.SetState(false);
                box.Colour = Color4.Transparent;
            }

            base.OnKeyUp(e);
        }
    }
}
