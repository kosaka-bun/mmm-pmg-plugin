using System;
using System.Collections.Generic;
using DxMath;
using MikuMikuPlugin;
using PianoPlayingMotionGenerator.Util.FingeringCalculator;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace PianoPlayingMotionGenerator.HandModel {

/// <summary>
/// 用于记录某个骨骼的在动作运算过程中的当前所在位置与旋转位置
/// </summary>
public class BoneFrameData {

    //用于获取某个骨骼在某个关键帧的动作数据，或为其添加关键帧
    public MotionFrameCollection frames;

    //该骨骼当前位置
    public Vector3 position;

    //该骨骼当前旋转位置
    public Quaternion rotation;

    //当前所在音符号
    public int nowNoteNo;

    //当前或上一个演奏的音符信息
    //注：当前所在音符号并不一定与此演奏的音符信息相同，当前音符号也可以代表只是移到
    //而不弹下的音符
    public SingleNote noteOfPlaying;

    //是否正在使用中
    public bool inUse;

    public void saveNow(int frameNo) {
        frames.AddKeyFrame(new MotionFrameData(frameNo, position, rotation));
    }
}

/// <summary>
/// 手掌模型，定义与获取与手掌有关的操作对象
/// </summary>
public partial class Hand {

    //名称前缀（左、右）
    public string prefix;

    //要演奏的音符列表
    public List<Note> noteList;

    //指尖骨骼
    public BoneFrameData[] fingers = new BoneFrameData[6];

    //指3（指2）骨骼，此骨骼仅能旋转
    public BoneFrameData[] fingerJoints = new BoneFrameData[6];

    //手首F骨骼
    public BoneFrameData wristF = new BoneFrameData();

    //初始化骨骼，初始位置，和加载移动数据
    public Hand(Model model, string _prefix) {
        prefix = _prefix;
        if(!(prefix.Equals("左") || prefix.Equals("右")))
            throw new Exception("手掌名前缀有误");
        loadBones(model);
        reset();
        loadData();
    }

    private void loadBones(Model model) {
        //初始化骨骼集合
        for(var i = 1; i <= 5; i++) {
            fingers[i] = new BoneFrameData();
            fingerJoints[i] = new BoneFrameData();
        }
        //初始化FrameCollection
        fingers[1].frames = model.Bones[prefix + "親指先"].Layers[0].Frames;
        fingers[2].frames = model.Bones[prefix + "人指先"].Layers[0].Frames;
        fingers[3].frames = model.Bones[prefix + "中指先"].Layers[0].Frames;
        fingers[4].frames = model.Bones[prefix + "薬指先"].Layers[0].Frames;
        fingers[5].frames = model.Bones[prefix + "小指先"].Layers[0].Frames;
        fingerJoints[1].frames = model.Bones[prefix + "親指２"].Layers[0].Frames;
        fingerJoints[2].frames = model.Bones[prefix + "人指３"].Layers[0].Frames;
        fingerJoints[3].frames = model.Bones[prefix + "中指３"].Layers[0].Frames;
        fingerJoints[4].frames = model.Bones[prefix + "薬指３"].Layers[0].Frames;
        fingerJoints[5].frames = model.Bones[prefix + "小指３"].Layers[0].Frames;
        wristF.frames = model.Bones[prefix + "手首F"].Layers[0].Frames;
    }

    /// <summary>
    /// 将手指、手腕位置复原为第0帧的位置
    /// </summary>
    private void reset() {
        for(var finger = 1; finger <= 5; finger++) {
            //将每根手指的当前位置置于第0帧的位置
            fingers[finger].position = getPosOfFinger(finger, 0);
            fingers[finger].rotation = getRotOfFinger(finger, 0);
            fingerJoints[finger].rotation = fingerJoints[finger]
                .frames.GetFrame(0).Quaternion;
            //设置为未使用
            fingers[finger].inUse = false;
        }
        //初始化手腕
        wristF.position = wristF.frames.GetFrame(0).Position;
        wristF.rotation = wristF.frames.GetFrame(0).Quaternion;
        //初始化当前音符号
        if(prefix.Equals("左")) {
            fingers[1].nowNoteNo = MovingData.CENTER_C - 12;
            fingers[2].nowNoteNo = MovingData.CENTER_C - 12 + 2;
            fingers[3].nowNoteNo = MovingData.CENTER_C - 12 + 4;
            fingers[4].nowNoteNo = MovingData.CENTER_C - 12 + 5;
            fingers[5].nowNoteNo = MovingData.CENTER_C - 12 + 7;
            wristF.nowNoteNo = MovingData.CENTER_C - 12 + 4;
        } else if(prefix.Equals("右")) {
            fingers[1].nowNoteNo = MovingData.CENTER_C;
            fingers[2].nowNoteNo = MovingData.CENTER_C + 2;
            fingers[3].nowNoteNo = MovingData.CENTER_C + 4;
            fingers[4].nowNoteNo = MovingData.CENTER_C + 5;
            fingers[5].nowNoteNo = MovingData.CENTER_C + 7;
            wristF.nowNoteNo = MovingData.CENTER_C + 4;
        }
    }

    /// <summary>
    /// 获取某根手指在某一帧的位置
    /// </summary>
    /// <param name="finger"></param>
    /// <returns></returns>
    public Vector3 getPosOfFinger(int finger, int frameNo) {
        return fingers[finger].frames.GetFrame(frameNo).Position;
    }

    /// <summary>
    /// 获取某根手指在某一帧的旋转位置
    /// </summary>
    /// <param name="finger"></param>
    /// <returns></returns>
    public Quaternion getRotOfFinger(int finger, int frameNo) {
        return fingers[finger].frames.GetFrame(frameNo).Quaternion;
    }

    public Vector3 getPosOfWrist(int frameNo) {
        return wristF.frames.GetFrame(frameNo).Position;
    }

    public Quaternion getRotOfWrist(int frameNo) {
        return wristF.frames.GetFrame(frameNo).Quaternion;
    }

    /// <summary>
    /// 获取某手指在中央C的位置
    /// </summary>
    /// <param name="finger"></param>
    public Vector3 getFingerToCenterCPos(int finger) {
        //手指原始位置
        Vector3 pos = getPosOfFinger(finger, 0);
        if(prefix.Equals("左")) {
            return pos + (2 + finger) * whiteKeySpacing;
        } else if(prefix.Equals("右")) {
            return pos - (finger - 1) * whiteKeySpacing;
        } else {
            throw new Exception("手掌名前缀有误");
        }
    }

    /// <summary>
    /// 获取某手指在指定音符的位置
    /// </summary>
    /// <returns></returns>
    public Vector3 getFingerNotePosition(int finger, int note) {
        Vector3 centerCPos = getFingerToCenterCPos(finger);
        if(MovingData.isBlackKey(note)) {
            return centerCPos + (MovingData.getCountOfCenterCToWhiteKey(
                note - 1) * whiteKeySpacing + toBlackKeyRightSpacing[finger]);
        }
        return centerCPos + MovingData.getCountOfCenterCToWhiteKey(note) *
            whiteKeySpacing;
    }

    /// <summary>
    /// 获取手腕在指定音符的位置
    /// </summary>
    /// <param name="note">音符号</param>
    /// <returns></returns>
    public Vector3 getWristNotePosition(int note) {
        //获取手腕在中央C的位置
        Vector3 centerCPos;
        if(prefix.Equals("左")) {
            centerCPos = wristF.frames.GetFrame(0).Position +
                5 * whiteKeySpacing;
        } else if(prefix.Equals("右")) {
            centerCPos = wristF.frames.GetFrame(0).Position -
                2 * whiteKeySpacing;
        } else {
            throw new Exception("手掌名前缀有误");
        }

        if(MovingData.isBlackKey(note)) {
            return centerCPos + wristToBlackKeyRightSpacing +
                MovingData.getCountOfCenterCToWhiteKey(note - 1) * 
                whiteKeySpacing;
        } else {
            return centerCPos + MovingData.getCountOfCenterCToWhiteKey(note) *
                whiteKeySpacing;
        }
    }
}

/// <summary>
/// 记录计算移动数据时所需要的元数据
/// 这些数据适用于每只手，每个手指，不具备特殊性
/// </summary>
public static class MovingData {

    //中央C的音符编号，每高一个半音+1
    public const int CENTER_C = 60;

    //是否是黑键
    public static bool isBlackKey(int note) {
        while(note < CENTER_C) note += 12;
        while(note >= CENTER_C + 12) note -= 12;
        switch(note) {
            case CENTER_C + 1:
            case CENTER_C + 3:
            case CENTER_C + 6:
            case CENTER_C + 8:
            case CENTER_C + 10:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 计算中央C到某白键要经过的白键数
    /// </summary>
    /// <param name="note">音符号</param>
    /// <returns></returns>
    public static int getCountOfCenterCToWhiteKey(int note) {
        if(isBlackKey(note))
            throw new Exception("要计算的键不是白键");
        var count = 0;
        if(note >= CENTER_C) {
            while(note >= CENTER_C + 12) {
                count += 7;
                note -= 12;
            }
            switch(note) {
                case CENTER_C:
                    return count;
                case CENTER_C + 2:
                    return count + 1;
                case CENTER_C + 4:
                    return count + 2;
                case CENTER_C + 5:
                    return count + 3;
                case CENTER_C + 7:
                    return count + 4;
                case CENTER_C + 9:
                    return count + 5;
                case CENTER_C + 11:
                    return count + 6;
                default:
                    throw new Exception("音符号计算有误");
            }
        } else {
            while(note <= CENTER_C - 12) {
                count -= 7;
                note += 12;
            }
            switch(note) {
                case CENTER_C:
                    return count;
                case CENTER_C - 1:
                    return count - 1;
                case CENTER_C - 3:
                    return count - 2;
                case CENTER_C - 5:
                    return count - 3;
                case CENTER_C - 7:
                    return count - 4;
                case CENTER_C - 8:
                    return count - 5;
                case CENTER_C - 10:
                    return count - 6;
                default:
                    throw new Exception("音符号计算有误");
            }
        }
    }
}

}