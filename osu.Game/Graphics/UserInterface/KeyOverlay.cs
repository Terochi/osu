// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    public partial class KeyOverlay : Container
    {
        private Key? targetKey;

        public Key? TargetKey
        {
            get => targetKey;
            set
            {
                if (targetKey == value)
                    return;

                targetKey = value;
            }
        }

        private ToggleGraph graph = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Colour = ColourInfo.GradientVertical(Color4.Transparent, Color4.White);

            RelativeSizeAxes = Axes.Y;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Size = new Vector2(30, 0.8f);
            Children = new Drawable[]
            {
                graph = new ToggleGraph
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(1),
                    Direction = BarDirection.BottomToTop
                }
            };
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat && e.Key == TargetKey)
                graph.SetState(true);

            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == TargetKey)
                graph.SetState(false);

            base.OnKeyUp(e);
        }
    }
}
