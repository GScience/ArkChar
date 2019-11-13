using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace Arkchar
{
    public class FightCharUpdater :ICharUpdater
    {
        private Random _random = new Random();

        public bool hasSkill;
        public float lastTimeSkill;

        public void Init(SpineWindow spineWindow)
        {
            var animations = spineWindow.animationData.SkeletonData.Animations;
            hasSkill = animations.Find((animation => animation.Name == "Skill")) != null;

            spineWindow.animationState.SetAnimation(0, "Start", false);
            spineWindow.animationState.AddAnimation(0, "Idle", true, 0);
        }

        public void OnMousePress(SpineWindow spineWindow)
        {
            spineWindow.animationState.SetAnimation(0, "Attack", false);
            spineWindow.animationState.AddAnimation(0, "Idle", true, 0);
        }

        public void Update(SpineWindow spineWindow)
        {
            if (!hasSkill)
                return;

            if (_random.NextDouble() < 0.0001f * (SDL.SDL_GetTicks() / 1000.0f - lastTimeSkill))
            {
                lastTimeSkill = SDL.SDL_GetTicks() / 1000.0f;
                spineWindow.animationState.SetAnimation(0, "Skill", false);
                spineWindow.animationState.AddAnimation(0, "Idle", true, 0);
            }
        }
    }
}
