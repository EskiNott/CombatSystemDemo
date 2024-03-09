using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : EskiNottToolKit.MonoSingleton<UIManager>
{
    [SerializeField] Camera mainCamera;
    protected override void Awake()
    {
        base.Awake();
        HitCountShowTimer = new();
        AwakeNumbers();
        AwakeComboTable();
    }

    void Start()
    {
        HitCountShowTimer.TimerEnded += HideHitCount;
        LockPointEnable(false);
        EnemySituationEnable(false);
    }

    void Update()
    {
        LockPointReposition();
        UpdateHitCountShowCheck();
        HitCountShowTimer.Update();
        UpdateComboTable();
    }

    #region LockPoint
    [Header("LockPoint")]
    [SerializeField] Transform lockPoint;
    [SerializeField] Transform lockTarget;
    [SerializeField] float LockPointRepositionDelay;

    public void LockPointEnable(bool ifEnable)
    {
        lockTarget = ifEnable ? PlayerCharacter.GetLockTarget().GetLockPointTransform() : null;
        lockPoint.gameObject.SetActive(ifEnable);
        EnemySituationUpdate();
    }

    public bool IsLockPointEnabled()
    {
        return lockPoint.gameObject.activeSelf;
    }

    private void LockPointReposition()
    {
        if (!IsLockPointEnabled() || lockTarget == null) { return; }
        // Vector2 pos = mainCamera.WorldToScreenPoint(lockTarget.position);
        // Vector3 result = new Vector3(pos.x, pos.y, 0);
        Vector3 _result = World2Screen(lockTarget.position);
        lockPoint.DOMove(_result, LockPointRepositionDelay);
    }

    #endregion

    #region Player
    [Header("Player")]
    [SerializeField] Character PlayerCharacter;
    [field: SerializeField] Image PlayerHealthBarCurrent;
    [field: SerializeField] TextMeshProUGUI PlayerHealthText;
    [field: SerializeField] Image PlayerCourageBarCurrent;
    [field: SerializeField] TextMeshProUGUI PlayerCourageText;

    private void PlayerHealthUpdate()
    {
        PlayerHealthBarCurrent.DOFillAmount(PlayerCharacter.Health / PlayerCharacter.MaxHealth, 0.8f).SetEase(Ease.OutElastic);
        PlayerHealthText.text = ((int)PlayerCharacter.Health).ToString();
    }

    private void PlayerPowerUpdate()
    {
        PlayerCourageBarCurrent.DOFillAmount(PlayerCharacter.Courage / PlayerCharacter.MaxCourage, 0.5f);
        PlayerCourageText.text = ((int)PlayerCharacter.Courage).ToString();
    }

    #region Hit
    [Header("Hit")]
    [field: SerializeField] TextMeshProUGUI HitNumberText;
    [field: SerializeField] Transform HitNumberTrans;
    [field: SerializeField] Transform HitTextTrans;
    [field: SerializeField] Vector3 HitEnlargeVector;
    [field: SerializeField] Timer HitCountShowTimer;
    [field: SerializeField] float HitCountShowExpiredTime;
    private void UpdateHitCountShowCheck()
    {
        HitNumberText.text = ((int)((Hero)PlayerCharacter).HitCount).ToString();
        if (((Hero)PlayerCharacter).HitCount == 0)
        {
            if (HitCountShowTimer.Running) { return; }
            HitCountShowTimer.Begin(HitCountShowExpiredTime, Timer.TimerMode.InstantStop);
        }
        else
        {
            ShowHitCount();
            if (!HitCountShowTimer.Running) { return; }
            HitCountShowTimer.Pause();
        }

    }

    private void ShowHitCount()
    {
        HitTextTrans.gameObject.SetActive(true);
        HitNumberTrans.gameObject.SetActive(true);
    }

    private void HideHitCount()
    {
        HitTextTrans.gameObject.SetActive(false);
        HitNumberTrans.gameObject.SetActive(false);
    }

    private void PlayerHitUpdate()
    {
        HitNumberTrans.DOScale(Vector3.one, 0.6f).From(HitEnlargeVector).SetEase(Ease.OutElastic);
        HitTextTrans.DOScale(Vector3.one, 0.6f).From(HitEnlargeVector).SetEase(Ease.OutElastic);
    }

    #endregion

    #endregion

    #region Enemy
    [Header("Enemy")]
    [SerializeField] Transform EnemySituation;
    [field: SerializeField] Image EnemyHealthBarCurrent;
    [field: SerializeField] TextMeshProUGUI EnemyNameText;
    [field: SerializeField] List<Image> EnemyToughBarCurrent;

    [field: SerializeField] Transform BreakText;
    [field: SerializeField] Transform HardBreakText;

    public void EnemySituationEnable(bool ifEnable)
    {
        EnemySituation.gameObject.SetActive(ifEnable);
    }

    private void EnemySituationUpdate()
    {
        if (!IsLockPointEnabled()) { return; }
        Character _e = PlayerCharacter.GetLockTarget();
        EnemyHealthBarCurrent.DOFillAmount(_e.Health / _e.MaxHealth, 0.1f);
        EnemyNameText.text = _e.CharacterName;

        for (int index = 0; index < EnemyToughBarCurrent.Count; index++)
        {
            float fillAmount = Mathf.Clamp01((_e.Tough / _e.MaxTough - index * 0.2f) / 0.2f);
            EnemyToughBarCurrent[index]
            .DOFillAmount(fillAmount, 0.6f).SetEase(Ease.OutElastic);

        }
    }

    public void ShowBreakText()
    {
        CanvasGroup _cg = BreakText.GetComponent<CanvasGroup>();

        Sequence _seq = DOTween.Sequence();
        _cg.alpha = 0;

        BreakText.gameObject.SetActive(true);

        _seq.Append(_cg.DOFade(1, 0.3f)
        .SetEase(Ease.OutElastic));

        _seq.Insert(0, BreakText.DOLocalMoveX(15, 0.3f)
        .From()
        .SetEase(Ease.OutBack));

        _seq.AppendInterval(1f);

        _seq.Append(_cg.DOFade(0, 0.4f)
        .SetEase(Ease.OutQuad)
        .OnComplete(
            () =>
            {
                BreakText.gameObject.SetActive(false);
                _cg.alpha = 1;
            }));
    }

    public void ShowHardBreakText()
    {
        CanvasGroup _cg = HardBreakText.GetComponent<CanvasGroup>();

        Sequence _seq = DOTween.Sequence();
        _cg.alpha = 0;

        HardBreakText.gameObject.SetActive(true);

        _seq.Append(_cg.DOFade(1, 0.3f)
        .SetEase(Ease.OutElastic));

        _seq.Insert(0, HardBreakText.DOLocalMoveX(15, 0.3f)
        .From()
        .SetEase(Ease.OutBack));

        _seq.Insert(0.25f, HardBreakText
        .DOScale(Vector3.one * 1.3f, 0.2f)
        .SetEase(Ease.OutSine)
        .From());

        _seq.AppendInterval(3f);

        _seq.Append(_cg.DOFade(0, 0.4f)
        .SetEase(Ease.OutQuad)
        .OnComplete(
            () =>
            {
                HardBreakText.gameObject.SetActive(false);
                _cg.alpha = 1;
            }));
    }

    #endregion

    #region Numbers
    [Header("Numbers")]
    [SerializeField] GameObject toughNumberPrefab;
    [SerializeField] GameObject damageNumberPrefab;
    [SerializeField] Transform toughNumberParent;
    [SerializeField] Transform damageNumberParent;
    [SerializeField] ObjectPool toughNumberObjectPool;
    [SerializeField] ObjectPool damageNumberObjectPool;
    [SerializeField] float dmg_Jump_Max;
    [SerializeField] float dmg_Jump_Min;
    [SerializeField] float dmg_Move;
    [SerializeField] float NumberFadeTime;

    private void AwakeNumbers()
    {
        toughNumberObjectPool = new(toughNumberPrefab, 10, 5);
        damageNumberObjectPool = new(damageNumberPrefab, 10, 5);
    }

    public void ShowToughNumber(Vector3 position, int tough)
    {
        GameObject _go = toughNumberObjectPool.Get();
        RectTransform _rect = _go.GetComponent<RectTransform>();
        UINumber _uiNumber = _go.GetComponent<UINumber>();

        _rect.SetParent(toughNumberParent);
        _uiNumber.SetText(tough.ToString());

        Vector3 _originalPos = World2Screen(position);

        _rect.position = new Vector3(_originalPos.x, _originalPos.y, _originalPos.z);

        _rect.DOPunchScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f)
        .SetEase(Ease.InBounce);

        _uiNumber.Cg.DOFade(0, NumberFadeTime)
        .SetEase(Ease.InExpo)
        .OnComplete(() =>
        {
            _uiNumber.Cg.alpha = 1;
            toughNumberObjectPool.Return(_go);
        });
    }

    public void ShowDamageNumber(int damage)
    {
        GameObject _go = damageNumberObjectPool.Get();
        RectTransform _rect = _go.GetComponent<RectTransform>();
        UINumber _uiNumber = _go.GetComponent<UINumber>();
        _rect.SetParent(damageNumberParent);
        _rect.localPosition = Vector3.zero;
        _uiNumber.SetText(damage.ToString());

        float _x = UnityEngine.Random.Range(-dmg_Move, dmg_Move);
        float _jumpPower = UnityEngine.Random.Range(dmg_Jump_Min, dmg_Jump_Max);
        Vector3 _jumpEnd = new(_x, -500, 0);

        _rect.DOLocalJump(_jumpEnd, _jumpPower, 1, 1.2f);

        _uiNumber.Cg.DOFade(0, 1f)
        .SetEase(Ease.InExpo)
        .OnComplete(() =>
        {
            _uiNumber.Cg.alpha = 1;
            damageNumberObjectPool.Return(_go);
        });
    }

    public Vector3 World2Screen(Vector3 worldPos)
    {
        Vector3 _result = mainCamera.WorldToScreenPoint(worldPos);
        return new Vector3(_result.x, _result.y, 0);
    }

    #endregion

    #region Combo

    [Header("Combo")]
    [SerializeField] private Transform ComboElementParent;
    [SerializeField] private GameObject ComboGameObject;
    private ObjectPool ComboElementPool;
    private ActionConfig lastAC;

    private void AwakeComboTable()
    {
        ComboElementPool = new(ComboGameObject, 2, 1);
        lastAC = null;
    }

    private void UpdateComboTable()
    {
        ActionConfig _playerac = PlayerCharacter.GetActionControl().GetCurrentActionConfig();
        if (PlayerCharacter == null && _playerac == null) { return; }
        if (lastAC == _playerac) { return; }
        ComboElementPool.ReturnAll();
        foreach (var item in _playerac.NextActionSequence)
        {
            string _button = item.Command switch
            {
                CombatManager.ActionCommand.HeavyAttack => "RT",
                CombatManager.ActionCommand.LightAttack => "RB",
                _ => "-1",
            };

            if (_button == "-1") { continue; }

            GameObject _go = ComboElementPool.Get();
            _go.transform.SetParent(ComboElementParent);
            ComboTableElement _cte = _go.GetComponent<ComboTableElement>();
            _cte.SetActionText(item.actionConfig.ActionName);
            _cte.SetButtonText(_button);
        }
        lastAC = _playerac;
    }

    #endregion

    /// <summary>
    /// 判断物体是否在相机范围内（准备来说，是判断一个点是否在相机范围内）
    /// </summary>
    /// <param name="camera">相机</param>
    /// <param name="targetTrans">物体</param>
    /// <returns>是否在相机范围内</returns>
    static public bool IsCameraVisible(Camera camera, Transform targetTrans)
    {
        Vector3 viewPos = camera.WorldToViewportPoint(targetTrans.position);
        if (viewPos.x > 0 && viewPos.x < 1)
        {
            if (viewPos.y > 0 && viewPos.y < 1)
            {
                if (viewPos.z >= camera.nearClipPlane && viewPos.z <= camera.farClipPlane)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void UpdateUI_All()
    {
        UpdateUI_Player();
        UpdateUI_Hit();
        UpdateUI_Enemy();
    }

    public void UpdateUI_Player()
    {
        PlayerHealthUpdate();
        PlayerPowerUpdate();
    }

    public void UpdateUI_Hit()
    {
        PlayerHitUpdate();
    }

    public void UpdateUI_Enemy()
    {
        EnemySituationUpdate();
    }

}
