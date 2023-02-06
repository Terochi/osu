// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneKeyOverlay : OsuTestScene
    {
        public TestSceneKeyOverlay()
        {
            KeyOverlay graph;

            Children = new[]
            {
                graph = new KeyOverlay
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TargetKey = Key.X
                },
            };

            AddStep("Do nothing", () => { });

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
