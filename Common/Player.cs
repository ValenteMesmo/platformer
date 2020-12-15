﻿using Microsoft.Xna.Framework;

namespace Platformer.Desktop
{
    public enum State
    {
        Idle,
        WalkingRight,
        WalkingLeft,
        Jump,
        BreakJump,
        Fall,
    }

    public static class Player
    {
        public static GameObject Create(InputController input, ValueKeeper<State> state)
        {
            var obj = GameObject.Create();
            obj.Position.Y = -14000;
            obj.Identifier = Identifier.Player;

            var grounded = ValueKeeper<int>.Create();
            var facingRight = ValueKeeper<bool>.Create();

            var collider = Collider.Create(obj);
            collider.Area = new Rectangle(
                50 * Const.Scale
                , 4000
                , 10000
                , 20000 - 4000);
            collider.Handler = StopsWhenHitingBlocks.Create();

            collider = Collider.Create(obj);
            collider.Area = new Rectangle(50 * Const.Scale, 210 * Const.Scale, 100 * Const.Scale, 10 * Const.Scale);
            collider.Handler = DetectsIfGrounded.Create(grounded);

            var animation = Animation.Create();
            animation.Sprites.Add(Textures.idle);
            animation.Sprites.Add(Textures.walk);

            obj.RenderHandler = animation;
            obj.UpdateHandler = () =>
            {
                ChangeToIdle.Try(input, grounded, state);
                ChangeToWalkingLeft.Try(input, grounded, state);
                ChangeToWalkingRight.Try(input, grounded, state);
                ChangeToJumpState.Try(obj, input, grounded, state);
                ChangeToFallingState.Try(obj, grounded, state);

                UpdateGravity.Update(obj);
                UpdateVelocityUsingInputs.Update(obj, input, facingRight);
                UpdateJump.Update(obj, input, grounded);
                UpdatePlayerAnimation.Update(input, animation, facingRight, grounded);
                grounded.SetValue(grounded.GetValue().DecrementUntil(0));
            };

            obj.OnDestroy = () =>
            {
                grounded.Destroy();
            };

            return obj;
        }




    }
}
