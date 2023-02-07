// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
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
            KeyOverlay graph;

            Children = Enumerable.Range(0, 26).Select(i => new KeyOverlay(Key.A + i, Color4.FromHsv(new Vector4(i * 0.039f, 1, 1, 1)))
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.Centre,
                Position = new Vector2((i - 14) * 50, 0)
            }).ToArray();

            Add(graph = new KeyOverlay(Key.Semicolon, Color4.Red)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = new Vector2(50 * 13, 0)
            });

            AddStep("Do nothing", () => { });

            AddStep("Weeeee", () =>
            {
                for (int i = 0; i < 27; i++)
                {
                    int j = i;
                    Scheduler.Add(new ScheduledDelegate(() =>
                    {
                        if (j != 26)
                            InputManager.PressKey(Key.A + j);
                        if (j != 0)
                            InputManager.ReleaseKey(Key.A + j - 1);
                    }, Time.Current + i * 100));
                }
            });

            AddStep("Bottom to top", () => changeDirection(BarDirection.BottomToTop));
            AddStep("Top to bottom", () => changeDirection(BarDirection.TopToBottom));
            AddStep("Left to right", () => changeDirection(BarDirection.LeftToRight));
            AddStep("Right to left", () => changeDirection(BarDirection.RightToLeft));
            AddStep("Change size", () => graph.Size = new Vector2(0.2f));

            void changeDirection(BarDirection direction)
            {
                //graph.Direction = direction;
                if (direction <= BarDirection.RightToLeft)
                    graph.Size = new Vector2(0.9f, 0.1f);
                else
                    graph.Size = new Vector2(0.1f, 0.9f);
            }
        }
    }
}
