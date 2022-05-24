// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Difficulty.Preprocessing
{
    /// <summary>
    /// Wraps a <see cref="HitObject"/> and provides additional information to be used for difficulty calculation.
    /// </summary>
    public class DifficultyHitObject
    {
        private readonly IReadOnlyList<DifficultyHitObject> difficultyHitObjects;

        /// <summary>
        /// The position of this <see cref="DifficultyHitObject"/> in the <see cref="difficultyHitObjects"/> list.
        /// </summary>
        public int Position;

        /// <summary>
        /// The <see cref="HitObject"/> this <see cref="DifficultyHitObject"/> wraps.
        /// </summary>
        public readonly HitObject BaseObject;

        /// <summary>
        /// The last <see cref="HitObject"/> which occurs before <see cref="BaseObject"/>.
        /// </summary>
        public readonly HitObject LastObject;

        /// <summary>
        /// Amount of time elapsed between <see cref="BaseObject"/> and <see cref="LastObject"/>, adjusted by clockrate.
        /// </summary>
        public readonly double DeltaTime;

        /// <summary>
        /// Clockrate adjusted start time of <see cref="BaseObject"/>.
        /// </summary>
        public readonly double StartTime;

        /// <summary>
        /// Clockrate adjusted end time of <see cref="BaseObject"/>.
        /// </summary>
        public readonly double EndTime;

        /// <summary>
        /// Creates a new <see cref="DifficultyHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> which this <see cref="DifficultyHitObject"/> wraps.</param>
        /// <param name="lastObject">The last <see cref="HitObject"/> which occurs before <paramref name="hitObject"/> in the beatmap.</param>
        /// <param name="clockRate">The rate at which the gameplay clock is run at.</param>
        /// <param name="objects">The list of <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        public DifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, List<DifficultyHitObject> objects)
        {
            difficultyHitObjects = objects;
            Position = objects.Count;
            BaseObject = hitObject;
            LastObject = lastObject;
            DeltaTime = (hitObject.StartTime - lastObject.StartTime) / clockRate;
            StartTime = hitObject.StartTime / clockRate;
            EndTime = hitObject.GetEndTime() / clockRate;
        }

        public DifficultyHitObject Previous(int backwardsIndex) => difficultyHitObjects[Position - (backwardsIndex + 1)];

        public DifficultyHitObject Next(int forwardsIndex) => difficultyHitObjects[Position + (forwardsIndex + 1)];
    }
}
