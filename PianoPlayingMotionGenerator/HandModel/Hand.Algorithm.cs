using System;
using System.Collections.Generic;
using System.Linq;
using PianoPlayingMotionGenerator.Util.FingeringCalculator;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable InvertIf
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable InconsistentNaming

namespace PianoPlayingMotionGenerator.HandModel {

/// <summary>
/// 手指动作的核心算法部分
/// </summary>
public partial class Hand {
    
    //以下方法不分左右手

    /// <summary>
    /// 提前抬起某个手指，此时，应将此手指所演奏的音符的结束帧设置为提前抬起的帧
    /// </summary>
    /// <param name="finger"></param>
    /// <param name="frameNo"></param>
    public void liftFinger(int finger, int frameNo) {
        liftFinger(finger);
        fingers[finger].noteOfPlaying.offFrame = frameNo;
    }

    /// <summary>
    /// 抬起某个手指
    /// </summary>
    /// <param name="finger"></param>
    public void liftFinger(int finger) {
        if(!fingers[finger].inUse) return;
        liftFingerUnconditionally(finger);
        fingers[finger].inUse = false;
    }
    
    public void liftAll() {
        for(var i = 1; i <= 5; i++) {
            liftFinger(i);
        }
    }

    /// <summary>
    /// 抬起使用中的手指，并将抬起后的状态保存到offFrame处
    /// </summary>
    public void liftAllAndSave(int? offFrame = null) {
        for(var i = 1; i <= 5; i++) {
            if(!fingers[i].inUse) continue;
            SingleNote note = fingers[i].noteOfPlaying;
            if(offFrame == null) {
                offFrame = note.offFrame;
            }
            if(offFrame - 2 > note.onFrame) {
                fingers[i].saveNow(offFrame.Value - 2);
            }
            liftFinger(i);
            fingers[i].saveNow(offFrame.Value);
        }
    }

    /// <summary>
    /// 无条件抬指
    /// </summary>
    /// <param name="finger"></param>
    public void liftFingerUnconditionally(int finger) {
        if(MovingData.isBlackKey(fingers[finger].noteOfPlaying.note))
            fingers[finger].position -= pressBlackKeySpacing;
        else
            fingers[finger].position -= pressWhiteKeySpacing;
    }

    /// <summary>
    /// 弹下某个手指当前要演奏的音符
    /// </summary>
    /// <param name="finger"></param>
    /// <param name="times"></param>
    public void pressFinger(int finger, int times = 1) {
        if(fingers[finger].inUse) return;
        if(MovingData.isBlackKey(fingers[finger].noteOfPlaying.note))
            fingers[finger].position += pressBlackKeySpacing * times;
        else
            fingers[finger].position += pressWhiteKeySpacing * times;
        fingers[finger].inUse = true;
    }

    public void moveWristToNote(int note) {
        wristF.position = getWristNotePosition(note);
        wristF.nowNoteNo = note;
    }

    /// <summary>
    /// 将手指移动到某音符，并设置此手指要演奏的音符
    /// </summary>
    /// <param name="note"></param>
    public void moveFingerToNote(SingleNote note) {
        moveFingerToNote(note.finger, note.note);
        fingers[note.finger].noteOfPlaying = note;
    }

    /// <summary>
    /// 仅将手指移动到某音符
    /// </summary>
    /// <param name="finger"></param>
    /// <param name="note"></param>
    public void moveFingerToNote(int finger, int note) {
        fingers[finger].position = getFingerNotePosition(finger, note);
        fingers[finger].nowNoteNo = note;
    }

    /// <summary>
    /// 保存所有指尖骨骼，指3（2）骨骼和手首F骨骼的当前状态到某帧
    /// </summary>
    public void saveAll(int frameNo, params int[] ignoreFingers) {
        for(var i = 1; i <= 5; i++) {
            if(ignoreFingers.Contains(i)) continue;
            fingers[i].saveNow(frameNo);
            fingerJoints[i].saveNow(frameNo);
        }
        wristF.saveNow(frameNo);
    }
}

/// <summary>
/// 右手专用算法部分
/// </summary>
public partial class Hand {
    
    /// <summary>
    /// 基于右手，演奏一个音符，然后合理安排手指位置
    /// </summary>
    /// <param name="_note"></param>
    public void play(Note note) {
        Note prev = null, next = null;
        if(note.index > 0) prev = noteList[note.index - 1];
        if(note.index < noteList.Count - 1) next = noteList[note.index + 1];
        #region 判断当前音符类型
        switch(note) {
            case SingleNote sn:
                playSingleNote(sn, prev);
                break;
            case Chord chord:
                playChord(chord, prev);
                break;
        }
        #endregion
    }

    private void playSingleNote(SingleNote note, Note prev) {
        if(prev != null && note.onFrame - 1 > prev.offFrame) {
            liftAll();
            saveAll(note.onFrame - 1);
        }
        moveFingerToNote(note);
        arrangeFingers(note, prev);
        pressFinger(note.finger);
        saveAll(note.onFrame);
        liftAll();
        saveAll(note.offFrame);
    }
    
    private void playChord(Chord chord, Note prev) {
        if(prev != null && chord.onFrame - 1 > prev.offFrame) {
            liftAll();
            saveAll(chord.onFrame - 1);
        }
        foreach(SingleNote sn in chord.notes) {
            moveFingerToNote(sn);
        }
        arrangeFingers(chord, prev);
        foreach(int finger in chord.fingers) {
            pressFinger(finger);
        }
        saveAll(chord.onFrame);
        liftAll();
        saveAll(chord.offFrame);
    }

    /// <summary>
    /// 基于右手，按当前的每个手指的使用状态，排列手指，移动手腕
    /// </summary>
    private void arrangeFingers(Note note, Note prev) {
        var algorithm = new ArrangeFingerAlgorithm(
            this, note, prev);
        //调整
        algorithm.doArrange();
        moveWristToNote(algorithm.calcWristPos());
    }
}

internal class ArrangeFingerAlgorithm {

    private readonly Hand hand;

    private readonly Note prev, note;

    public ArrangeFingerAlgorithm(Hand hand, Note note, Note prev) {
        this.hand = hand;
        this.prev = prev;
        this.note = note;
    }

    //调整手指顺序
    public void doArrange() {
        //确定固定的手指
        var fixedFingers = new HashSet<int>();
        fixedFingers.UnionWith(note.fingers);
        //To be continued...
    }

    /// <summary>
    /// 判断某两个手指所在音符的音程是否大于或小于适当的音程差，若是则进行调整
    /// lowFirst表示当两个手指均未使用时
    /// </summary>
    public void arrangeInterval(int low, int high, bool lowFirst) {
        int lowNoteNo = hand.fingers[low].nowNoteNo,
            highNoteNo = hand.fingers[high].nowNoteNo;
        if(lowNoteNo >= highNoteNo) {
            if(lowFirst) {
                hand.moveFingerToNote(low, highNoteNo - 
                    getMaxInterval(low, high));
            } else {
                hand.moveFingerToNote(high, lowNoteNo + 
                    getMaxInterval(low, high));
            }
            return;
        }
        if(highNoteNo - lowNoteNo > getMaxInterval(low, high)) {
            if(lowFirst) {
                hand.moveFingerToNote(low, highNoteNo - 
                    getMaxInterval(low, high));
            } else {
                hand.moveFingerToNote(high, lowNoteNo + 
                    getMaxInterval(low, high));
            }
            return;
        }
        if(highNoteNo - lowNoteNo < getMinInterval(low, high)) {
            if(lowFirst) {
                hand.moveFingerToNote(low, highNoteNo - 
                    getMinInterval(low, high));
            } else {
                hand.moveFingerToNote(high, lowNoteNo + 
                    getMinInterval(low, high));
            }
            return;
        }
    }

    //获取手指间最大间距
    public static int getMaxInterval(int low, int high) {
        switch(low) {
            case 1:
                switch(high) {
                    case 2: return Interval.PERFECT5;
                    case 3: return Interval.SMALL7;
                    case 4: return Interval.PERFECT8;
                    case 5: return Interval.PERFECT8;
                }
                break;
            case 2:
                switch(high) {
                    case 3: return Interval.BIG3;
                    case 4: return Interval.PERFECT4;
                    case 5: return Interval.PERFECT5;
                }
                break;
            case 3:
                switch(high) {
                    case 4: return Interval.SMALL3;
                    case 5: return Interval.PERFECT5;
                }
                break;
            case 4:
                switch(high) {
                    case 5: return Interval.SMALL3;
                }
                break;
        }
        throw new NotSupportedException();
    }

    public static int getMinInterval(int low, int high) {
        switch(low) {
            case 1:
                switch(high) {
                    case 2: return Interval.SMALL2;
                    case 3: return Interval.SMALL3;
                    case 4: return Interval.PERFECT4;
                    case 5: return Interval.PERFECT5;
                }
                break;
            case 2:
                switch(high) {
                    case 3: return Interval.SMALL2;
                    case 4: return Interval.SMALL3;
                    case 5: return Interval.PERFECT4;
                }
                break;
            case 3:
                switch(high) {
                    case 4: return Interval.SMALL2;
                    case 5: return Interval.SMALL3;
                }
                break;
            case 4:
                switch(high) {
                    case 5: return Interval.SMALL2;
                }
                break;
        }
        throw new NotSupportedException();
    }

    //确定手腕位置
    public int calcWristPos() {
        var pitch = 0;
        //判断5指所在音符的均值
        for(var i = 1; i <= 5; i++) {
            pitch += hand.fingers[i].nowNoteNo;
        }
        pitch /= 5;
        //拇指在黑键上，但音符均值不是黑键
        if(MovingData.isBlackKey(hand.fingers[1].nowNoteNo) &&
           !MovingData.isBlackKey(pitch)) {
            //若均值+1为黑键，则取+1的值，否则取+2的值
            pitch = MovingData.isBlackKey(pitch + 1) ? pitch + 1 : pitch + 2;
        }
        //拇指不在黑键上，但均值是黑键
        else if(!MovingData.isBlackKey(hand.fingers[1].nowNoteNo) &&
                MovingData.isBlackKey(pitch)) {
            //取+1的值
            pitch += 1;
        }
        //其他情况，取默认值
        else {
            //none
        }
        return pitch;
    }
}

}