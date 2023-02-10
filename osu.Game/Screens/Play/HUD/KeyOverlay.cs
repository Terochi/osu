// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Play.HUD
{
    public partial class KeyOverlay : Container, ISkinnableDrawable
    {
        [SettingSource("Target key")]
        public Bindable<OverlayKey> TargetKey { get; set; } = new Bindable<OverlayKey>(OverlayKey.X);

        [SettingSource("Time span", "How long it takes to get from start to end in milliseconds")]
        public Bindable<float> TimeSpan { get; private set; } = null!;

        public enum OverlayKey
        {
            X = Key.X,
            Y = Key.Y
        }

        private ToggleGraph graph = null!;
        private OsuSpriteText text = null!;
        private Box box = null!;

        public Bindable<Color4> GraphColour = new Bindable<Color4>();

        [Resolved]
        private ReadableKeyCombinationProvider keyCombinationProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(30, 300);
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(30, 30),
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
                            Colour = Color4.White
                        },
                    }
                },
                graph = new ToggleGraph
                {
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Size = new Vector2(30, 270),
                }
            };
        }

        protected override void LoadComplete()
        {
            TargetKey.BindValueChanged(e =>
                text.Text = keyCombinationProvider.GetReadableString(KeyCombination.FromKey((Key)e.NewValue)), true);

            GraphColour.BindValueChanged(e => graph.Colour = e.NewValue, true);

            TimeSpan = graph.GraphTimeSpan.GetBoundCopy();

            base.LoadComplete();
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat && e.Key == (Key)TargetKey.Value)
            {
                graph.SetState(true);
                box.Colour = GraphColour.Value;
            }

            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == (Key)TargetKey.Value)
            {
                graph.SetState(false);
                box.Colour = Color4.Transparent;
            }

            base.OnKeyUp(e);
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
