using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace Arkchar
{
    public class BuildCharUpdater : ICharUpdater
    {
        private Random _random = new Random();

        public float xOffset;
        public float movingSpeed = 5f;
        public float moveScale;
        public bool isMovingAround;

        public float lastTimeMove;

        public bool hasSleep;

        public void Init(SpineWindow spineWindow)
        {
            var animations = spineWindow.animationData.SkeletonData.Animations;
            hasSleep = animations.Find((animation => animation.Name == "Sleep")) != null;

            lastTimeMove = SDL.SDL_GetTicks() / 1000.0f;

            if (hasSleep)
                spineWindow.animationState.SetAnimation(0, "Sleep", false);
            spineWindow.animationState.AddAnimation(0, "Relax", true, 0);
        }

        public void OnMousePress(SpineWindow spineWindow)
        {
            var current = spineWindow.animationState.GetCurrent(0).Animation;
            spineWindow.animationState.SetAnimation(0, "Interact", false);
            spineWindow.animationState.AddAnimation(0, current.Name == "Move" ? "Move" : "Relax", true, 0);
        }

        public void Update(SpineWindow spineWindow)
        {
            if (!isMovingAround &&
                _random.NextDouble() < 0.0001f * (SDL.SDL_GetTicks() / 1000.0f - lastTimeMove))
            {
                lastTimeMove = SDL.SDL_GetTicks() / 1000.0f;
                isMovingAround = true;
                spineWindow.animationState.SetAnimation(0, "Move", true);
            }

            if (isMovingAround && spineWindow.animationState.GetCurrent(0).Animation.Name == "Move")
            {
                moveScale += 0.2f;
                if (moveScale >= 1)
                    moveScale = 1;

                xOffset += movingSpeed * moveScale;
                spineWindow.Left += movingSpeed * moveScale;

                spineWindow.skeleton.FlipX = movingSpeed < 0;

                if (xOffset > 100)
                    movingSpeed *= -1;
                else if (xOffset < 0)
                {
                    isMovingAround = false;
                    movingSpeed *= -1;
                    spineWindow.animationState.SetAnimation(0, "Relax", true);
                    xOffset = 0;
                }
            }
            else
            {
                moveScale = 0;
            }
        }
    }
}
