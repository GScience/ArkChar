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

        public float lastTimeSkill;

        public void Init(SpineWindow spineWindow)
        {
            spineWindow.animation.SetAnimation(0, "Start", false);
            spineWindow.animation.AddAnimation(0, "Idle", true, 0);
        }

        public void OnMousePress(SpineWindow spineWindow)
        {
            spineWindow.animation.SetAnimation(0, "Attack", false);
            spineWindow.animation.AddAnimation(0, "Idle", true, 0);
        }

        public void Update(SpineWindow spineWindow)
        {
            if (_random.NextDouble() < 0.0001f * (SDL.SDL_GetTicks() / 1000.0f - lastTimeSkill))
            {
                lastTimeSkill = SDL.SDL_GetTicks() / 1000.0f;
                spineWindow.animation.SetAnimation(0, "Skill", false);
                spineWindow.animation.AddAnimation(0, "Idle", true, 0);
            }
        }
    }
}
