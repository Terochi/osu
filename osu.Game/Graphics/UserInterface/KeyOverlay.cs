// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
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

        private BarDirection direction = BarDirection.LeftToRight;

        public BarDirection Direction
        {
            get => direction;
            set
            {
                if (direction == value)
                    return;

                direction = value;

                updateDirection();
            }
        }

        private Vector2 directionVector
        {
            get
            {
                switch (Direction)
                {
                    default:
                    case BarDirection.LeftToRight:
                        return new Vector2(1, 0);

                    case BarDirection.RightToLeft:
                        return new Vector2(-1, 0);

                    case BarDirection.TopToBottom:
                        return new Vector2(0, 1);

                    case BarDirection.BottomToTop:
                        return new Vector2(0, -1);
                }
            }
        }

        private ToggleGraph graph = null!;

        private OsuSpriteText text = null!;

        private Box box = null!;

        private readonly Color4 color;

        [Resolved]
        private ReadableKeyCombinationProvider keyCombinationProvider { get; set; } = null!;

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
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(30),
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = 3.5f,
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
                            Text = keyCombinationProvider.GetReadableString(KeyCombination.FromKey(TargetKey))
                        },
                    }
                },
                graph = new ToggleGraph
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(1),
                    Position = new Vector2(0, -30),
                    Direction = BarDirection.BottomToTop,
                    Colour = color,
                }
            };
        }

        private void updateDirection()
        {
            Vector2 dir = directionVector;
            graph.Position = dir * 15;
            RelativeSizeAxes = (Axes)(int)(Math.Abs(dir.X) + Math.Abs(dir.Y) * 2);
            Size = new Vector2(Math.Abs(dir.Y) * 29.2f + 0.8f, Math.Abs(dir.X) * 29.2f + 0.8f);
            graph.Anchor = (Anchor)((int)Math.Pow(2, dir.X + 4) + (int)Math.Pow(2, dir.Y + 1));
            graph.Direction = direction;
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
