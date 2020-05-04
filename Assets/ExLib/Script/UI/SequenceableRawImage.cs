using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ExLib.UI
{
    [RequireComponent(typeof(RawImage))]
    public class SequenceableRawImage : MonoBehaviour
    {
        public enum SequenceSourceType
        {
            Textures,
            SpriteSheet,
        }

        [System.Serializable]
        public class EnterFrameEvent : UnityEvent<int> { }

        [SerializeField]
        private SequenceSourceType _type;

        [SerializeField]
        private Texture2D[] _sequences;

        [SerializeField]
        private Texture2D _sheet;

        [SerializeField]
        private int _row;

        [SerializeField]
        private int _column;

        [SerializeField]
        private int _frameRate;

        [SerializeField]
        private bool _reverse;

        [SerializeField]
        private bool _loop;

        [SerializeField]
        private bool _yoyo;

        [SerializeField]
        private float _repeatDelay;

        [SerializeField]
        private bool _playOnAwake;

        private RawImage _image;

        private float _elapseTime = 0f;

        private float _frameDeltaTime;

        private bool _yoyoReverse;

        private float _repeatDelayTime;

        private int _direction = 1;

#if UNITY_EDITOR
        public float FrameRatio { get; set; }
#endif

        public UnityEvent onComplete;
        public EnterFrameEvent onEnterFrame;

        public RawImage Viewport { get { if (_image == null) _image = GetComponent<RawImage>(); return _image; } }
        public int CurrentFrame { get; set; }

        public int TotalFrames { get; private set; }
        public bool IsReverse { get { return _reverse || _yoyoReverse; } }

        public RectTransform rectTransform { get { return transform as RectTransform; } }

        public float Duration
        {
            get
            {
                if (_frameDeltaTime == 0f)
                    _frameDeltaTime = (1f / (float)_frameRate);

                return TotalFrames * _frameDeltaTime;
            }
        }

        public bool IsPlaying { get; private set; }

        private int _startFrame;
        private int _reverseStartFrame;
        public int StartFrame { get { return _startFrame; } set { _startFrame = value < 1 ? 1 : value; } }
        public int ReverseStartFrame { get { return _reverseStartFrame; } set { _reverseStartFrame = value < 1 ? 1 : value; } }


        void Awake()
        {
            TotalFrames = _type == SequenceSourceType.Textures ? _sequences.Length : _row * _column;
            _frameDeltaTime = 1f / (float)_frameRate;
            _image = GetComponent<RawImage>();

            StartFrame = StartFrame == 0 ? 1 : StartFrame;
            ReverseStartFrame = ReverseStartFrame == 0 ? TotalFrames : ReverseStartFrame;

            _direction = _reverse ? -1 : 1;
            CurrentFrame = _reverse ? ReverseStartFrame : StartFrame;
        }

        private void OnEnable()
        {
            Rewind();

            if (_playOnAwake)
                Play();
        }

        void Update()
        {
            if (_image == null)
                _image = GetComponent<RawImage>();

            if (!IsPlaying)
            {
                CurrentFrame = Mathf.Clamp(CurrentFrame, 1, TotalFrames);
                if (_type == SequenceSourceType.Textures)
                {
                    _image.texture = _sequences[CurrentFrame - 1];
                    _image.SetMaterialDirty();
                }
                else
                {

                }
                return;
            }

            if ((_direction > 0 && CurrentFrame > TotalFrames) || (_direction < 0 && CurrentFrame < 1))
            {
                if (_loop)
                {
                    if (_yoyo)
                    {
                        if ((_reverse && !_yoyoReverse) || (!_reverse && _yoyoReverse))
                        {
                            if (_repeatDelayTime <= _repeatDelay)
                            {
                                _elapseTime = 0f;
                                _repeatDelayTime += Time.deltaTime;
                                return;
                            }
                            _repeatDelayTime = 0f;
                        }

                        if (onComplete != null)
                            onComplete.Invoke();

                        _direction *= -1;
                        _yoyoReverse = !_yoyoReverse;

                    }
                    else
                    {
                        if (_repeatDelayTime <= _repeatDelay)
                        {
                            _repeatDelayTime += Time.deltaTime;
                            return;
                        }

                        if (onComplete != null)
                            onComplete.Invoke();

                        CurrentFrame = _reverse ? TotalFrames + 1 : 0;
                        _repeatDelayTime = 0f;
                    }
                }
                else
                {
                    IsPlaying = false;

                    if (onComplete != null)
                        onComplete.Invoke();
                    return;
                }
            }

            CurrentFrame = Mathf.Clamp(CurrentFrame, 1, TotalFrames);

            if (_type == SequenceSourceType.Textures)
            {
                _image.texture = _sequences[CurrentFrame - 1];
                _image.SetMaterialDirty();
            }
            else if (_type == SequenceSourceType.SpriteSheet)
            {

            }

            if (onEnterFrame != null && _elapseTime == 0f)
                onEnterFrame.Invoke(CurrentFrame);

            _elapseTime += Time.deltaTime;

            if (_elapseTime < _frameDeltaTime)
                return;

            CurrentFrame += (int)(_elapseTime / _frameDeltaTime) * _direction;

            _elapseTime = _elapseTime > _frameDeltaTime ? 0f : _elapseTime;
        }

        public void SetSequences(Texture2D[] sequences)
        {
            if (_type != SequenceSourceType.Textures)
                return;

            _sequences = sequences;
        }

        public void SetSequences(Texture2D sheet)
        {
            if (_type != SequenceSourceType.SpriteSheet)
                return;
        }

        public void Reverse(bool value)
        {
            _reverse = value;

            if (!_playOnAwake && !IsPlaying)
            {
                _direction = _reverse ? -1 : 1;
                CurrentFrame = _reverse ? ReverseStartFrame : StartFrame;
            }
            else
            {
                if (_yoyo)
                    _yoyoReverse = !_yoyoReverse;
                _repeatDelayTime = _repeatDelay;
                _direction *= -1;
            }
        }

        public void Rewind()
        {
            IsPlaying = false;

            _direction = _reverse ? -1 : 1;
            CurrentFrame = _reverse ? ReverseStartFrame : StartFrame;

            _repeatDelayTime = 0f;
            _elapseTime = 0f;
        }

        public void Play()
        {
            IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying = false;
        }

#if UNITY_EDITOR
        public void ShowFrame(float value)
        {
            _image = GetComponent<RawImage>();
            TotalFrames = _type == SequenceSourceType.Textures ? _sequences.Length : _row * _column;

            int frame = (int)((float)(TotalFrames - 1) * value);

            if (_type == SequenceSourceType.Textures)
            {
                _image.texture = _sequences[frame];
                _image.SetMaterialDirty();
            }
            else if (_type == SequenceSourceType.SpriteSheet)
            {

            }
        }
#endif
    }
}
