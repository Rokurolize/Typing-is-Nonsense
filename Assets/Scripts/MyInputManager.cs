using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif

/// <summary>
///  �v���b�g�t�H�[���ˑ��̃L�[�{�[�h���͎󂯎����s���AEventBus �ɒʒm�𔭍s����
///  ���̌�A���̃N���X����A�ʒm�����ꂽ�ۂɎ��s���Ăق������\�b�h�� EventBus �Ƀf���Q�[�g�Ƃ��ēo�^����
/// </summary>
public class MyInputManager : MonoBehaviour
{
    // �V�t�g�Ǘ��B�V�t�g�L�[�� 2 ����̂ŁAbool �łȂ� int
    // ���\�b�h���ϐ��i���͎󂯎���̊m�肵�� shifted�j�Ƌ�ʂ��邽�߁A�t�B�[���h�Ƃ��Ă� shifted �ɂ� _ �t�^
    private static int _shifted = 0;

    // ScriptableObject
    [SerializeField]
    KeyBind keyBind;
    KeyBindDicts dicts;


    void Awake()
    {
        keyBind = new KeyBind();
        keyBind.LoadFromJson(0);
        dicts = new KeyBindDicts(keyBind);

#if UNITY_STANDALONE_WIN
        Debug.Log("standalone win");
        var workInBackground = false;
        RawKeyInput.Start(workInBackground);

        RawKeyInput.OnKeyDown += RawKeyDownHandlerForWin;
        RawKeyInput.OnKeyDown += KeyDownHandlerForWin;
        RawKeyInput.OnKeyUp += KeyUpHandlerForWin;
#endif

        EventBus.Instance.SubscribeKeyBindChanged(KeyBindChangedHandler);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDisable()
    {
#if UNITY_STANDALONE_WIN
        RawKeyInput.OnKeyDown -= RawKeyDownHandlerForWin;
        RawKeyInput.OnKeyDown -= KeyDownHandlerForWin;
        RawKeyInput.OnKeyUp -= KeyUpHandlerForWin;

        RawKeyInput.Stop(); // ����Ȃ��� Unity Editor ��������
#endif

        EventBus.Instance.UnsubscribeKeyBindChanged(KeyBindChangedHandler);
    }

    // �v���b�g�t�H�[���ˑ��̃L�[���̓C�x���g�n���h��
    // �v���b�g�t�H�[���ˑ��̃L�[�R�[�h���A���[�U�ŗL�� KeyID �ɕϊ����ăv���b�g�t�H�[����ˑ��������Ă�
#if UNITY_STANDALONE_WIN
    private void RawKeyDownHandlerForWin(RawKey key)
    {
        EventBus.Instance.NotifyRawKeyDown(key);
    }
    private void KeyDownHandlerForWin(RawKey key)
    {
        if (!dicts.dictToKeyID_FromRawKey.ContainsKey(key)) {
            Debug.Log($"No KeyID for RawKey : {key.ToString()}");
            return;
        }
        else OnKeyDown(dicts.ToKeyID_FromRawKey(key));
    }
    private void KeyUpHandlerForWin(RawKey key)
    {
        if (dicts.dictToKeyID_FromRawKey.ContainsKey(key))
        {
            ushort keyID = dicts.ToKeyID_FromRawKey(key);
            if (keyID == 0 || keyID == 1) OnShiftKeyUp();
        }
    }
#endif

    public static ushort GetShiftState()
    {
        return (ushort)_shifted;
    }

    // EventBus �ɃC�x���g������ʒm���郁�\�b�h�Q
    /// KeyID �����ꕶ�� 100 ~ �ł���΁A���ꂼ��̃��\�b�h���Ă�
    ///         �V�t�g 0 | 1 �ł���΁AOnShiftKeyDown() �����s
    ///         �ʏ핶�� 2 ~ 50 �ł���΁A�V�t�g���ʌ� OnNormalKeyDown() �����s
    ///         �� �������ACharID ������U���Ă��Ȃ� 2 �L�[�̏ꍇ�́A���������s���Ȃ�
    private void OnKeyDown(ushort keyID)
    {
        ushort retkey = keyID;
        if (retkey == 0 || retkey == 1) OnShiftKeyDown(); // RawKey:16
        else if (retkey == 100) OnReturnKeyDown(); // RawKey:13
        else if (retkey == 101) OnEscKeyDown(); // RawKey:27
        // KeyID:2 (Preset:Space) �ɂ̓V�t�g�֌W�����Ȃ̂ŁA�V�t�g�����Ɉڍs�����Ȃ�
        else if (retkey == 2) OnNormalKeyDown(0); // CharID of Space: 0
        else
        {
            bool shifted = false;
            if (_shifted > 0) shifted = true;
            if (shifted) retkey += 48;

            if (!dicts.dictToCharID_FromKeyID.ContainsKey(retkey))
                Debug.Log($"KeyID {retkey} �� CharID ���A�T�C������Ă��܂���B");
            else OnNormalKeyDown(dicts.ToCharID_FromKeyID(retkey));
        }

    }
    /// CharID �̃A�T�C������Ă��Ȃ��L�[�����͖�������΂����̂ŁA
    private void OnNormalKeyDown(ushort charID)
    {
        EventBus.Instance.NotifyNormalKeyDown(charID);
    }
    private void OnShiftKeyDown()
    {
        _shifted++;
        if (_shifted > 2) _shifted = 2;
        EventBus.Instance.NotifyShiftKeyDown();
    }
    private void OnShiftKeyUp()
    {
        _shifted--;
        if (_shifted < 0) _shifted = 0;
        EventBus.Instance.NotifyShiftKeyUp();
    }
    private void OnReturnKeyDown()
    {
        EventBus.Instance.NotifyReturnKeyDown();
    }
    private void OnEscKeyDown()
    {
        EventBus.Instance.NotifyEscKeyDown();
    }

    // �C�x���g�n���h��
    private void KeyBindChangedHandler()
    {
        keyBind.LoadFromJson(0);
        dicts = new KeyBindDicts(keyBind);
    }


}
