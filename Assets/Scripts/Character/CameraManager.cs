using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using EskiNottToolKit;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    [SerializeField] List<CinemachineVirtualCamera> VirtualCameras;
    CinemachineVirtualCamera currentCamera;
    [SerializeField] Character PlayerCharacter;
    CinemachineBasicMultiChannelPerlin characterCameraPerlin;
    Transform playerTrans;
    [field: SerializeField] Timer playerActionTimer;
    [SerializeField] Timer ShakeTimer;

    protected override void Awake()
    {
        base.Awake();
        playerTrans = PlayerCharacter.transform;
        ShakeTimer = new();
        characterCameraPerlin = VirtualCameras[0].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Start()
    {
        StartScreenShake();
        SelectCamera(0);
        playerActionTimer = PlayerCharacter.GetActionControl().ActionTimer;
    }

    private void Update()
    {
        ShakeTimer.Update();
    }

    public void SelectCamera(int index)
    {
        if (index >= VirtualCameras.Count) { return; }
        foreach (var item in VirtualCameras)
        {
            item.gameObject.SetActive(false);
        }
        VirtualCameras[index].gameObject.SetActive(true);
        currentCamera = VirtualCameras[index];
    }

    public void DoExecuteCamera()
    {
        VirtualCameras[1].transform.position =
        new Vector3(playerTrans.position.x, 1f, playerTrans.position.z);
        VirtualCameras[1].transform.position += playerTrans.forward * -2f + playerTrans.right * 1;
        Vector3 _targetpos = PlayerCharacter.GetLockTarget().GetTransform().position;
        Vector3 _pos = new(_targetpos.x, 1f, _targetpos.z);
        VirtualCameras[1].transform.LookAt(_pos);
        playerActionTimer.TimerEnded += DoExecuteCamera_reset;
        SelectCamera(1);
    }

    private void DoExecuteCamera_reset()
    {
        SelectCamera(0);
        playerActionTimer.TimerEnded -= DoExecuteCamera_reset;
    }

    private void StartScreenShake()
    {
        ShakeTimer.TimerEnded += ScreenShakeStop;
    }

    public void ScreenShake(Vector3 direction, float power, float duration)
    {
        characterCameraPerlin = currentCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        characterCameraPerlin.m_AmplitudeGain = power;
        characterCameraPerlin.m_PivotOffset = direction;
        ShakeTimer.Begin(duration, Timer.TimerMode.InstantStop);
    }
    private void ScreenShakeStop()
    {
        characterCameraPerlin.m_AmplitudeGain = 0;
    }
}
