﻿using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// Traditional Sonic rolling. Makes you not as slow going uphill and even faster going downhill.
    /// </summary>
    public class Roll : Move
    {
        #region Controls
        /// <summary>
        /// Input string used for activation.
        /// </summary>
        [SerializeField]
        [Tooltip("Input string used for activation.")]
        public string ActivateInput;

        /// <summary>
        /// Whether to activate when the input is in the opposite direction (if ActivateInput is "Vertical" and this
        /// is true, activates when input moves down instead of up).
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to activate when the input is in the opposite direction (if ActivateInput is \"Vertical\" " +
                 "and this is true, activates when input moves down instead of up.")]
        public bool RequireNegative;

        /// <summary>
        /// Minimum ground speed required to start rolling, in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Minimum ground speed required to start rolling, in units per second.")]
        public float MinActivateSpeed;
        #endregion
        #region Physics
        /// <summary>
        /// Slope gravity when rolling uphill, in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("Slope gravity when rolling uphill, in units per second squared.")]
        public float UphillGravity;

        /// <summary>
        /// Slope gravity when rolling downhill, in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("Slope gravity when rolling downhill, in units per second squared.")]
        public float DownhillGravity;

        /// <summary>
        /// Deceleration while rolling, in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("Deceleration while rolling, in units per second squared.")]
        public float Deceleration;

        /// <summary>
        /// Friction while rolling, in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("Friction while rolling, in units per second squared.")]
        public float Friction;
        #endregion
        private bool _rightDirection;

        private float _originalSlopeGravity;
        private float _originalFriction;
        private float _originalDeceleration;

        public override void Reset()
        {
            base.Reset();

            ActivateInput = "Vertical";
            RequireNegative = true;
            MinActivateSpeed = 0.61875f;

            UphillGravity = 2.8125f;
            DownhillGravity = 11.25f;
            Friction = 0.8451f;
            Deceleration = 4.5f;
        }

        public override void Awake()
        {
            base.Awake();

            _rightDirection = false;
        }

        public override bool Available()
        {
            return Mathf.Abs(Controller.GroundVelocity) > MinActivateSpeed;
        }

        public override bool InputActivate()
        {
            return RequireNegative
                ? Input.GetAxis(ActivateInput) < 0.0f
                : Input.GetAxis(ActivateInput) > 0.0f;
        }

        public override bool InputDeactivate()
        {
            return DMath.Equalsf(Controller.GroundVelocity) || !Controller.Grounded ||
                (_rightDirection && Controller.GroundVelocity < 0.0f) ||
                (!_rightDirection && Controller.GroundVelocity > 0.0f);
        }

        public override void OnActiveEnter(State previousState)
        {
            _rightDirection = Controller.GroundVelocity > 0.0f;

            _originalSlopeGravity = Controller.SlopeGravity;
            _originalFriction = Controller.GroundFriction;
            _originalDeceleration = Controller.GroundControl.Deceleration;

            Controller.AutoRotate = false;
            Controller.GroundFriction = Friction;
            Controller.GroundControl.AccelerationLocked = true;
            Controller.GroundControl.Deceleration = Deceleration;
        }

        public override void OnActiveFixedUpdate()
        {
            bool uphill = false;
            if (Controller.GroundVelocity > 0.0f)
            {
                uphill = DMath.AngleInRange_d(Controller.RelativeSurfaceAngle, 0.0f, 180.0f);
            } else if (Controller.GroundVelocity < 0.0f)
            {
                uphill = DMath.AngleInRange_d(Controller.RelativeSurfaceAngle, 180.0f, 360.0f);
            }

            Controller.SlopeGravity = uphill ? UphillGravity : DownhillGravity;
        }

        public override void OnActiveExit()
        {
            Controller.AutoRotate = true;
            Controller.SlopeGravity = _originalSlopeGravity;
            Controller.GroundFriction = _originalFriction;
            Controller.GroundControl.AccelerationLocked = false;
            Controller.GroundControl.Deceleration = _originalDeceleration;
        }
    }
}