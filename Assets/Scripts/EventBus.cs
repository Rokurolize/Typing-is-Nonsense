using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif

/// <summary>
/// �N���X EventBus
/// �e�N���X���烊�A���^�C���ɃC�x���g�����ʒm���󂯎��A�o�^���ꂽ�f���Q�[�g�����s����
///  https://indie-du.com/entry/2017/05/26/130000
/// </summary>
public class EventBus
{
    // Singleton
    private static EventBus s_instance;
    public static EventBus Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new EventBus();
            }
            return s_instance;
        }
    }
    private EventBus()
    {

    }

    // ���̃N���X���u�������ʒm���󂯎�������Ɏ��s���Ăق����R�[���o�b�N�֐��v�̓o�^�����邽�߂̃f���Q�[�g
    public delegate void OnNormalKeyDown(ushort keyID);
    public delegate void OnReturnKeyDown();
    public delegate void OnEscKeyDown();
    private event OnNormalKeyDown _onNormalKeyDown;
    private event OnReturnKeyDown _onReturnKeyDown;
    private event OnEscKeyDown _onEscKeyDown;
#if UNITY_STANDALONE_WIN
    public delegate void OnRawKeyDown(RawKey key);
    private event OnRawKeyDown _onRawKeyDown;
#endif

    // ���̃N���X���ʒm�󂯎��i��EventBus�ւ̃f���Q�[�g�o�^�j�����邽�߂̃��\�b�h
    public void SubscribeNormalKeyDown(OnNormalKeyDown onNormalKeyDown)
    {
        _onNormalKeyDown += onNormalKeyDown;
    }
    public void SubscribeReturnKeyDown(OnReturnKeyDown onReturnKeyDown)
    {
        _onReturnKeyDown += onReturnKeyDown;
    }
    public void SubscribeEscKeyDown(OnEscKeyDown onEscKeyDown)
    {
        _onEscKeyDown += onEscKeyDown;
    }
#if UNITY_STANDALONE_WIN
    public void SubscribeRawKeyDown(OnRawKeyDown onRawKeyDown)
    {
        _onRawKeyDown += onRawKeyDown;
    }
#endif

    // ���̃N���X���ʒm�󂯎����������邽�߂̃��\�b�h
    public void UnsubscribeNormalKeyDown(OnNormalKeyDown onNormalKeyDown)
    {
        _onNormalKeyDown -= onNormalKeyDown;
    }
    public void UnsubscribeReturnKeyDown(OnReturnKeyDown onReturnKeyDown)
    {
        _onReturnKeyDown -= onReturnKeyDown;
    }
    public void UnsubscribeEscKeyDown(OnEscKeyDown onEscKeyDown)
    {
        _onEscKeyDown -= onEscKeyDown;
    }
#if UNITY_STANDALONE_WIN
    public void UnsubscribeRawKeyDown(OnRawKeyDown onRawKeyDown)
    {
        _onRawKeyDown -= onRawKeyDown;
    }
#endif

    // ���̃N���X����AEventBus �ɒʒm���˗����郁�\�b�h
    // char c ��n���̂͏璷�iMyInputManager.CharMap �����邩��j�����A�֗������Ȃ̂Ŏc���Ă���
    public void NotifyNormalKeyDown(ushort keyID)
    {
        if (_onNormalKeyDown != null) _onNormalKeyDown(keyID);
    }
    public void NotifyReturnKeyDown()
    {
        if (_onReturnKeyDown != null) _onReturnKeyDown();
    }
    public void NotifyEscKeyDown()
    {
        if (_onEscKeyDown != null) _onEscKeyDown();
    }

#if UNITY_STANDALONE_WIN
    public void NotifyRawKeyDown(RawKey key)
    {
        if (_onRawKeyDown != null) _onRawKeyDown(key);
    }
#endif
}
