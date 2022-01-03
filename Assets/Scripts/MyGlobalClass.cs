/// �Q�[���S�̂Ŏg�p����N���X��\���̂��`

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq; // �z�񏉊����p

public class TrialData
{
    // �~�X���~�b�g
    public ushort missLimit = 50;
    // �������ł����L�[�� = ���łL�[ID�B0 ����n�܂� assignmentLength �őŐ�
    public int typedKeys = 0;
    // ���݉����b�v�ڂ� 1-indexed
    public int nowLap = 1;
    // �g�[�^���^�C���E���b�v�^�C���E�e����Ō��^�C��
    // ���ꂼ��̃^�C���E�~�X�͑S�āu���v�^�C���v�œ����̂ŁA�Ō����Ԃ��o���ɂ͈����Z���K�v
    // lap, key �̔z��� [0] �͔ԕ�
    public long totalTime = 0;
    public long[] lapTime;
    public long[] keyTime;
    // �g�[�^���~�X�E���b�v�~�X�E�e�L�[�ӂ�~�X�i�v�邩�H�j
    // ���ꂼ��̃^�C���E�~�X�͑S�āu���v�^�C���v�œ����̂ŁA�Ō����Ԃ��o���ɂ͈����Z���K�v
    // lap, key �̔z��� [0] �͔ԕ�
    public int totalMiss = 0;
    public int[] lapMiss;
    public int[] keyMiss;

    public ushort[] trialAssignment_CharID;
    public char[] trialAssignment_Char;

    public TrialData(ushort lim, int assignmentLength, int numOfLaps)
    {
        Debug.Log("New trialData was instantiated.");

        missLimit = lim;

        lapTime = Enumerable.Repeat<long>(0, numOfLaps + 1).ToArray();
        keyTime = Enumerable.Repeat<long>(0, assignmentLength + 1).ToArray();

        lapMiss = Enumerable.Repeat<int>(0, numOfLaps + 1).ToArray();
        keyMiss = Enumerable.Repeat<int>(0, assignmentLength + 1).ToArray();

        trialAssignment_CharID = new ushort[assignmentLength];
        trialAssignment_Char = new char[assignmentLength];
    }
}