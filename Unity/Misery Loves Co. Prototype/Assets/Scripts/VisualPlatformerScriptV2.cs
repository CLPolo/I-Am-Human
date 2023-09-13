using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameFeel
{
    public class VisualPlatformerScriptV2 : BasicPlatformerScriptV2
    {
        [Header("Jump Deformation")]
        [SerializeField] private Vector2 jumpDeformScale;
        [SerializeField] private AnimationCurve jumpDeformCurve;
        
        [Header("Landing Deformation")]
        [SerializeField] private float landingDeformTime=0.1f;
        [SerializeField] private Vector2 landingDeformScale;
        [SerializeField] private AnimationCurve landingDeformCurve;

        [Header("Camera Parameters"),Tooltip("You'll have to replay to see these changes in effect")] 
        [SerializeField] private CameraControl cameraControl;
        [SerializeField] private bool zoomOnImpact;
        [SerializeField] private bool zoomOnJump;
        [SerializeField] private bool shakeOnImpact;
        [SerializeField] private bool shakeOnJump;
        private readonly  UnityEvent _impactEvent = new UnityEvent();
        private readonly  UnityEvent _jumpEvent = new UnityEvent();

        [Header("Particle Parameters")] 
        [SerializeField] private bool dustOnImpact = false;
        [SerializeField] private bool dustOnJump = false;
        [SerializeField] private bool dustOnDirectionChange = false;
        
        private Vector3 _originalSpritePosition;
        private Vector3 _originalSpriteScale;
        
        private GameObject _spriteObject;
        private Transform _spriteTransform;
        private ParticleSystem _particleSystem;
        private Coroutine _landingDeformRoutine;
        private Coroutine _jumpDeformRoutine;
        protected override void Start() {
            base.Start();
            
            //Cache sprite data
            _spriteObject = transform.GetChild(0).gameObject;
            _spriteTransform = _spriteObject.transform;
            _originalSpritePosition = _spriteTransform.localPosition;
            _originalSpriteScale = _spriteTransform.localScale;
            
            //Cache Particle System
            _particleSystem = transform.GetChild(1).GetComponent<ParticleSystem>();
            
            //Subscribe Event
            if (zoomOnImpact) _impactEvent.AddListener(cameraControl.Zoom);
            if (shakeOnImpact) _impactEvent.AddListener(cameraControl.Shake);
            if (zoomOnJump) _jumpEvent.AddListener(cameraControl.Zoom);
            if (shakeOnJump) _jumpEvent.AddListener(cameraControl.Shake);
        }

        protected override void Update()
        {
            var oldVelocity = _currentVelocity;
            base.Update();
            if (SignFloat(_currentVelocity.x) != SignFloat(oldVelocity.x) &&
                dustOnDirectionChange)
            {
                //direction changed
                if(dustOnDirectionChange) _particleSystem.Play();
            }
        }

        protected override void OnGrounded_Hook()
        {
            base.OnGrounded_Hook();
            if (_jumpDeformRoutine != null)
            {
                StopCoroutine(_jumpDeformRoutine);
                _jumpDeformRoutine = null;
                _spriteTransform.localScale = _originalSpriteScale;
                _spriteTransform.localPosition = _originalSpritePosition;
            }
            _landingDeformRoutine = StartCoroutine(_Deform(landingDeformScale,landingDeformCurve,landingDeformTime));
            if(dustOnImpact) _particleSystem.Play();
            _impactEvent.Invoke();
        }

        protected override void OnJump_Hook()
        {
            base.OnJump_Hook();
            if (_landingDeformRoutine != null)
            {
                StopCoroutine(_landingDeformRoutine);
                _landingDeformRoutine = null;
                _spriteTransform.localScale = _originalSpriteScale;
                _spriteTransform.localPosition = _originalSpritePosition;
            }
            else if (_jumpDeformRoutine != null)
            {
                StopCoroutine(_jumpDeformRoutine);
                _jumpDeformRoutine = null;
                _spriteTransform.localScale = _originalSpriteScale;
                _spriteTransform.localPosition = _originalSpritePosition;
            }
            _jumpDeformRoutine = StartCoroutine(_Deform(jumpDeformScale,jumpDeformCurve,jumpTime));
            if(dustOnJump) _particleSystem.Play();
            _jumpEvent.Invoke();
        }
        
        private IEnumerator _Deform(Vector2 deformScale, AnimationCurve curve,float maxTime)
        {
            float timer = 0;
            while (timer <= maxTime)
            {
                //Calculating deformed scale
                float pingPongVal = Mathf.PingPong(timer, (maxTime/2f))/(maxTime/2f);
                float curveValue = curve.Evaluate(pingPongVal);
                Vector2 newScale = new Vector2(
                    Mathf.Lerp(_originalSpriteScale.x, _originalSpriteScale.x * deformScale.x, curveValue),
                    Mathf.Lerp(_originalSpriteScale.y, _originalSpriteScale.y * deformScale.y, curveValue));
                _spriteTransform.localScale = newScale;
                
                timer += Time.deltaTime;
                yield return null;
            }
            _spriteTransform.localScale = _originalSpriteScale;
            _spriteTransform.localPosition = _originalSpritePosition;
        }
    }
    
}
