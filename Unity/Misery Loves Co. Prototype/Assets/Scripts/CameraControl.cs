using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeel{
    public class CameraControl : MonoBehaviour
    {
        [Header("Motion")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Vector2 cameraOffset;
        [SerializeField] private float cameraFollowSpeed;
        [SerializeField] private float cameraEaseTime=0.47f;

        [Header("Shake")]
        [SerializeField] private float shakeDuration=0.5f;
        [SerializeField] private float shakeStrength;
        [SerializeField] private AnimationCurve shakeCurve;
        
        [Header("Zoom")]
        [SerializeField] private float zoomTime=0.5f;
        [SerializeField] private float zoomStrength;
        [SerializeField] private AnimationCurve zoomCurve;

        private Vector3 _target = Vector3.zero;
        private Vector3 _cameraSpeed;
        private bool _shaking;
        private bool _zoomed;

        private Camera _camera;
        private float _defaultRotation;
        private float _defaultZoom;

        private Coroutine _zoomRoutine = null;
        private Coroutine _shakeRoutine = null;

        private void Start()
        {
            _camera = Camera.main;
            _target = _updateTarget();
            transform.position = _target;
            _cameraSpeed = new Vector3(cameraFollowSpeed, cameraFollowSpeed, 0);
            
            _defaultZoom = _camera.orthographicSize;
        }
        
        void Update()
        {
            _target = _updateTarget();
            transform.position = Vector3.SmoothDamp(transform.position, _target, ref _cameraSpeed, cameraEaseTime);
        }

        private Vector3 _updateTarget()
        {
            var targetPosition = cameraTarget.position;
            targetPosition = new Vector3(
                targetPosition.x - cameraOffset.x,
                targetPosition.y + cameraOffset.y,
                transform.position.z);
            return targetPosition;
        }
        
        public void Zoom()
        {
            if (_zoomRoutine != null)
            {
                StopCoroutine(_zoomRoutine);
                _zoomRoutine = null;
            }

            _zoomRoutine = StartCoroutine(_zoom());
        }
        public void Shake()
        {
            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
                _shakeRoutine = null;
            }

            _shakeRoutine = StartCoroutine(_shake());
        }
        
        private IEnumerator _zoom()
        {
            float zoomTimer = 0;
            while (zoomTimer<=zoomTime)
            {
                zoomTimer += Time.deltaTime;
                float current = Mathf.PingPong(zoomTimer, zoomTime/2f)/(zoomTime/2f);
                print(zoomTimer);
                float curveVal = zoomCurve.Evaluate(current);
                _camera.orthographicSize = Mathf.Lerp(_defaultZoom, zoomStrength, curveVal);;
                yield return null;
            }
        }
        private IEnumerator _shake()
        {
            float shakeTimer = 0;
            while (shakeTimer<=shakeDuration)
            {
                shakeTimer += Time.deltaTime;
                float current = Mathf.Lerp(1, 0, shakeTimer / shakeDuration);
                float radius = shakeCurve.Evaluate(current)*shakeStrength;
                Vector2 offset = Random.insideUnitCircle * radius;
                _camera.transform.localPosition = new Vector3(offset.x,offset.y,_camera.transform.localPosition.z);
                yield return null;
            }
        }
    }
}