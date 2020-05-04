// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using ExLib.Utils;

namespace ExLib.UI
{
    public class ThrobberAlter : ThrobberBase
    {
        [SerializeField]
        private Vector2 _orbSize;
        [SerializeField]
        private Texture2D _orbTexture;

        [SerializeField]
        private Color _orbColor;

        public bool AutoStart = true;

        private GameObject[] _orbs;
        private Vector3 _centerPoint = new Vector3();

        [Tooltip("The axis for the orbit")]
        public Vector3 Axis = Vector3.forward;
        [Tooltip("Radius of the orbit")]
        public float Radius = 0.075f;
        [Tooltip("Speed of the orbit")]
        public float RevolutionSpeed = 1.9f;
        [Tooltip("How many revolutions per cycle")]
        public int Revolutions = 3;
        [Tooltip("The space or angle between each element")]
        public float AngleSpace = 12;

        [Tooltip("Are we paused?")]
        public bool IsPaused = false;
        [Tooltip("smooth easing or linear revolutions")]
        public bool SmoothEaseInOut = false;
        [Tooltip("If smooth easing, how smooth?")]
        public float SmoothRatio = 0.65f;

        /// <summary>
        /// Internal functional values
        /// </summary>
        // current angle
        private float _angle = 0;
        //current time
        private float _time = 0;
        // current revolution count
        private int _revolutionsCount = 0;
        // is it time to pause or setup the next cycle?
        private bool _loopPause = false;
        // the currently fading element
        private int _fadeIndex = 0;
        // check the loopPause next Update
        private bool _checkLoopPause = false;
        // The center position
        private Vector3 _positionVector;
        // the rotation vector during the animation
        private Vector3 _rotatedPositionVector;

        // the loader is starting
        private bool _startingLoader = false;
        // the index of the Orbs to start with
        private int _startingIndex;

        private RawImage[] _orbImgs;

        private const int ORBS_COUNT = 8;

        /// <summary>
        /// setup all the orbs
        /// </summary>
        private void Awake()
        {
            CreateOrbs();
        }

        private void CreateOrbs()
        {
            _orbImgs = new RawImage[ORBS_COUNT];
            _orbs = new GameObject[ORBS_COUNT];
            for (int i = 0; i < ORBS_COUNT; ++i)
            {
                _orbs[i] = new GameObject("Orb_" + i);
                _orbImgs[i] = _orbs[i].AddComponent<RawImage>();
                RectTransform rect = _orbs[i].transform as RectTransform;
                rect.SetParent(transform);
                rect.localScale = Vector3.one;
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _orbSize.x);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _orbSize.y);
                _orbImgs[i].texture = _orbTexture;
                _orbImgs[i].color = _orbColor;
            }
            IsGenerated = true;
        }

        /// <summary>
        /// setup the position of the animation and elements
        /// </summary>
        void Start()
        {
            _positionVector = -transform.up;

            if (!Mathf.Approximately(Vector3.Angle(Axis, _positionVector), 90))
            {
                _positionVector = transform.forward;
                if (!Mathf.Approximately(Vector3.Angle(Axis, _positionVector), 90))
                {
                    float x = Mathf.Abs(Axis.x);
                    float y = Mathf.Abs(Axis.y);
                    float z = Mathf.Abs(Axis.z);

                    if (x > y && x > z)
                    {
                        // left or right - cross with the z axis
                        _positionVector = Vector3.Cross(Axis * Radius, transform.forward);
                    }

                    if (z > y && z > x)
                    {
                        // forward or backward - cross with the x axis
                        _positionVector = Vector3.Cross(Axis * Radius, transform.right);
                    }

                    if (y > z && y > x)
                    {
                        // up or down - cross with the x axis
                        _positionVector = Vector3.Cross(Axis * Radius, transform.right);
                    }
                }
            }

            if (AutoStart)
            {
                StartLoader();
            }
        }

        /// <summary>
        /// Starting the loading animation
        /// </summary>
        public void StartLoader()
        {
            _startingLoader = true;
            _startingIndex = 0;
            _revolutionsCount = 0;
            IsPaused = false;
        }

        /// <summary>
        /// stopping the loading animation
        /// </summary>
        public void StopLoader()
        {
            if (_orbs == null)
                return;

            for (int i = 0; i < _orbs.Length; ++i)
            {
                _orbs[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// expose a method to resume
        /// </summary>
        public void ResumeOrbit()
        {
            IsPaused = false;
        }

        /// <summary>
        /// reset the angle of the animation without restarting
        /// </summary>
        public void ResetOrbit()
        {
            _angle = 0;
        }

        /// <summary>
        /// easing function
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public float QuartEaseInOut(float s, float e, float v)
        {
            //e -= s;
            if ((v /= 0.5f) < 1)
                return e / 2 * v * v * v * v + s;

            return -e / 2 * ((v -= 2) * v * v * v - 2) + s;
        }

        /// <summary>
        /// Animate the loader
        /// </summary>
        void Update()
        {
            if (IsPaused) return;

            float percentage = _time / RevolutionSpeed;

            for (int i = 0; i < _orbs.Length; ++i)
            {
                GameObject orb = _orbs[i];

                // get the revolution completion percentage
                float orbPercentage = percentage - AngleSpace / 360 * i;
                if (orbPercentage < 0)
                {
                    orbPercentage = 1 + orbPercentage;
                }

                if (SmoothEaseInOut)
                {
                    float linearSmoothing = 1 * (orbPercentage * (1 - SmoothRatio));
                    orbPercentage = QuartEaseInOut(0, 1, orbPercentage) * SmoothRatio + linearSmoothing;
                }

                // set the angle
                _angle = 0 - (orbPercentage) * 360;

                if (_startingLoader)
                {
                    if (orbPercentage >= 0 && orbPercentage < 0.5f)
                    {
                        if (i == _startingIndex)
                        {
                            orb.SetActive(true);
                            if (i >= _orbs.Length - 1)
                            {
                                _startingLoader = false;
                            }
                            _startingIndex += 1;
                        }
                    }
                }

                // apply the values
                //orb.transform.Rotate(Axis, mAngle);
                _rotatedPositionVector = Quaternion.AngleAxis(_angle, Axis) * _positionVector * Radius;
                orb.transform.localPosition = _centerPoint + _rotatedPositionVector;

                // check for looping and handle loop counts
                if (_checkLoopPause != _loopPause)
                {
                    if (_loopPause && orbPercentage > 0.25f)
                    {
                        if (i == _fadeIndex)
                        {
                            _orbImgs[i].CrossFadeAlpha(0f, .2f, true);
                            /*FadeColors fade = orb.GetComponent<FadeColors>();
                            fade.FadeOut(false);*/
                            if (i >= _orbs.Length - 1)
                            {
                                _checkLoopPause = _loopPause;
                            }
                            _fadeIndex += 1;
                        }

                    }

                    if (!_loopPause && orbPercentage > 0.5f)
                    {
                        if (i == _fadeIndex)
                        {
                            _orbImgs[i].CrossFadeAlpha(1f, .2f, true);
                            /*FadeColors fade = orb.GetComponent<FadeColors>();
                            fade.FadeIn(false);*/
                            if (i >= _orbs.Length - 1)
                            {
                                _checkLoopPause = _loopPause;
                            }
                            _fadeIndex += 1;
                        }
                    }

                }
            }

            _time += Time.deltaTime;
            if (!_loopPause)
            {
                if (_time >= RevolutionSpeed)
                {
                    _time = _time - RevolutionSpeed;

                    _revolutionsCount += 1;

                    if (_revolutionsCount >= Revolutions && Revolutions > 0)
                    {
                        _loopPause = true;
                        _fadeIndex = 0;
                        _revolutionsCount = 0;
                    }
                }
            }
            else
            {
                if (_time >= RevolutionSpeed)
                {
                    _time = 0;
                    _revolutionsCount += 1;
                    if (_revolutionsCount >= Revolutions * 0.25f)
                    {
                        _fadeIndex = 0;
                        _loopPause = false;
                        _revolutionsCount = 0;
                    }
                }
            }
        }

        public override void Play()
        {
            StartLoader();
        }

        public override void Stop()
        {
            StopLoader();
        }
    }
}