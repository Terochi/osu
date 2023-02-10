// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneKeyOverlay : OsuManualInputManagerTestScene
    {
        public TestSceneKeyOverlay()
        {
            const int key_count = 10;

            int offset = 0;

            KeyOverlay[] overlays;

            Children = overlays = Enumerable.Range(0, key_count).Select(i => new KeyOverlay
            {
                GraphColour = { Value = Color4.FromHsv(new Vector4((float)i / key_count, 1, 1, 1)) },
                TargetKey = { Value = (KeyOverlay.OverlayKey)Key.A + i },
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = new Vector2((i - key_count / 2) * 50, 0)
            }).ToArray();

            AddSliderStep("Time spans", 1000f, 5000f, 1000f, val =>
                overlays.ForEach(o => o.TimeSpan.Value = val));

            AddSliderStep("Colours", 0f, 0.99f, 0f, val =>
                overlays.ForEach(o => o.GraphColour.Value = Color4.FromHsv(new Vector4(val, 1, 1, 1))));

            AddSliderStep("Keys", 0, 26 - key_count, 0, val =>
            {
                offset = val;

                for (int i = 0; i < key_count; i++)
                    overlays[i].TargetKey.Value = (KeyOverlay.OverlayKey)Key.A + i + offset;
            });

            AddStep("Animation", () =>
            {
                for (int i = 0; i < key_count; i++)
                {
                    int j = i;

                    Scheduler.Add(new ScheduledDelegate(() => InputManager.PressKey(Key.A + j + offset), Time.Current + j * 100));

                    Scheduler.Add(new ScheduledDelegate(() => InputManager.ReleaseKey(Key.A + j + offset), Time.Current + (j + 1) * 100));
                }
            });
        }
    }
}
