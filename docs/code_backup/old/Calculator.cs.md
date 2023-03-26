这是带有序列的指法计算器的备份，现有算法并不使用序列

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
// ReSharper disable InconsistentNaming
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace PianoPlayingMotionGenerator.Util.FingeringCalculator {

/// <summary>
/// 提供音符列表信息，为某只手计算用于此列表的合适的指法
/// </summary>
public partial class FingeringCalculator {

    /// <summary>
    /// 计算指法，返回计算好指法的Note列表
    /// </summary>
    /// <returns></returns>
    public List<Note> calculate() {
        //遍历音符列表
        for (var i = 0; i < noteList.Count; i++) {
            //判断当前音符类型
            switch (noteList[i]) {
                //单音
                case SingleNote _:
                    var sn = noteList[i] as SingleNote;
                    //如果没有下一个音，或下一个音不是单音
                    if (!hasNextNote(i) || !(noteList[i + 1] is SingleNote)) {
                        //TODO 寻找离此音最近的手指，安排指法
                        if(!hasNextNote(i)) return noteList;
                        else continue;
                    }
                    //有下一个音，且下一个音是单音
                    var nextNote = noteList[i + 1] as SingleNote;
                    //根据下一个音的音高，封装合适的音符序列
                    NoteSequence sequence = null;
                    if (nextNote.note > sn.note) {
                        //下一个音符号大于本音符号，则应当处理为递增音符序列
                        //递增序列
                        sequence = getNoteSequence(ref i, NoteSequence.Type.INCREASE);
                    } else if (nextNote.note < sn.note) {
                        //递减序列
                        sequence = getNoteSequence(ref i, NoteSequence.Type.DECREASE);
                    } else if (nextNote.note == sn.note) {
                        //同音序列
                        sequence = getNoteSequence(ref i, NoteSequence.Type.SAME_NOTE);
                    }
                    //处理音符序列
                    calculateSequence(sequence);
                    break;
                //复音
                case Chord _:
                    var chord = noteList[i] as Chord;
                    //TODO 写一个方法计算复音指法，根据前缀
                    break;
            }
        }
        return noteList;
    }

    /// <summary>
    /// 根据手的前缀，来计算一个音符序列的指法
    /// </summary>
    /// <param name="seq"></param>
    public void calculateSequence(NoteSequence seq) {
        switch (prefix) {
            case "左":
                switch (seq.type) {
                    case NoteSequence.Type.INCREASE:
                        break;
                    case NoteSequence.Type.DECREASE:
                        break;
                    case NoteSequence.Type.SAME_NOTE:
                        break;
                }
                break;
            case "右":
                switch (seq.type) {
                    case NoteSequence.Type.INCREASE:
                        calcIncreaseSeq(seq);
                        break;
                    case NoteSequence.Type.DECREASE:
                        break;
                    case NoteSequence.Type.SAME_NOTE:
                        break;
                }
                break;
            default:
                throw new Exception("手掌名前缀有误");
        }
    }

    /// <summary>
    /// 可用于计算右手上行序列、左手下行序列（将下行序列倒置后按右手上行序列来处理）
    /// </summary>
    /// <param name="seq"></param>
    private void calcIncreaseSeq(NoteSequence seq) {
        //每计算一个音的指法，就将此音传递给模型弹奏，然后再继续计算下一个音的指法，这样可以获取到准确的离音符最近的手指信息
        //获取序列指定位置的音符，若越界返回null
        SingleNote getNote(int index) {
            try {
                return seq.notes[index];
            } catch (Exception e) {
                return null;
            }
        }
        //遍历序列中的音
        for (var i = 0; i < seq.notes.Count; i++) {
            //获取当前位置的音符
            SingleNote note = getNote(i);
            
            /*//不需要跳跃到达，调用此方法
            void calc() {
                //若此音是序列中第一个音
                if (i == 0) {
                    Note _prevNote = noteList[note.index - 1];
                    switch (_prevNote) {
                        case SingleNote prevNote:
                            //判断前一个音是否低于当前音
                            if (prevNote.note < note.note) {
                                calcHigherByLower(prevNote, note, calcJumpTo);
                            } else {
                                
                            }
                            break;
                        case Chord prevChord:
                            break;
                    }
                    return;
                }
            }*/
            
            //根据前一个音的音高与指法计算当前音的指法
            //若当前音是音符列表的第一个音，则需要跳跃到达当前音
            if (note.index == 0) {
                calcJumpToNote(note, seq, i);
            } 
            //若不是，则需结合前一个音来判断
            else {
                //前一个音
                Note prev = noteList[note.index - 1];
                //判断前一个音的类型
                switch (prev) {
                    case SingleNote prevNote:
                        //若前一个音是单音，且低于当前音
                        if (prevNote.note < note.note) {
                            //如果前一个音是1指，且与序列中最高音的音程小于或等于八度，且当前音处于倒数第四个或更往后的位置，则序列可以不用再考虑转指
                            if (prevNote.finger == 1 &&
                                prevNote.getNoteDistance(
                                    seq.notes[seq.notes.Count - 1]) <= 12 &&
                                i >= seq.notes.Count - 4) {
                                seq.shouldTurn = false;
                            }
                            //将前一个音和当前音交由方法判断指法
                            //calcHigherByLower(prevNote, note, calcJumpTo, seq.shouldTurn);
                        }
                        else {
                            
                        }
                        break;
                    case Chord prevChord:
                        break;
                }
            }
            
            /*//为当前音计算指法，忽略它的后一个音（主要用于要跳跃到的音的前一个音）
            void calcWithoutNext() {
                
            }
            //若当前音位于由低到高的三个音的中间位置，调用此方法计算当前音的指法与演奏类型
            //这里的左音不一定是序列内的，所以需要传入
            void calcMidNote(SingleNote left) {
                //若左音与本音的音程超过大九度，使用跳跃
                if (note.getNoteDistance(left) > 14) {
                    //此音是否是序列中最后一个音，若是，用5指，否则忽略左音进行计算
                    if (i == seq.notes.Count - 1)
                        note.finger = 5;
                    else 
                        calcWithoutPrev();
                    note.type = Note.PlayType.JUMP;
                }
                
            }
            
            //若此音是整个音符列表的第一个音，则仅能通过后一个音来判断
            if (note.index == 0) {
                calcWithoutPrev();
            }
            //不是列表中第一个音
            //若此音是序列中第一个音，则需综合前一个音的类型与音高，和后一个音的音高，综合判断
            else if (i == 0) {
                //序列至少包含2个音，且此音不是列表第一个音，则前一个音与后一个音必定存在
                Note _prevNote = noteList[note.index - 1];
                SingleNote nextNote = getNote(i + 1);
                //判断前一个音的类型
                switch (_prevNote) {
                    case SingleNote prevNote:
                        //第一个音的前一个音的音高低于此音，则前一个音必属于一个下行序列
                        //前一个音是否低于本音
                        if (prevNote.note < note.note) {
                            //计算由低至高的三个单音中位于中间的音的指法与演奏类型
                            calcMidNote(prevNote);
                        }
                        break;
                    case Chord prevChord:
                        
                        break;
                }
            }
            //若不是第一个音，则需要综合前一个音与后一个音来判断，不确定时，可获取离这个音最近的手指*/
            
            //此音已安排指法，弹奏并继续下一次循环
            hand.press(note);
        }
    }

    /// <summary>
    /// 可用于计算右手下行序列与左手上行序列
    /// </summary>
    /// <param name="seq"></param>
    private void calcDecreaseSeq(NoteSequence seq) {
        
    }

    /// <summary>
    /// 左右手同音序列可使用一套算法
    /// </summary>
    /// <param name="seq"></param>
    private void calcSameNoteSeq(NoteSequence seq) {
        
    }

    /// <summary>
    /// 提供当前音符索引，和要构建的序列类型，将当前音符和之后若干个音符添加到序列中
    /// </summary>
    /// <param name="index"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public NoteSequence getNoteSequence(ref int index, NoteSequence.Type type) {
        var seq = new NoteSequence(type);
        //从当前位置开始，添加音符到序列中，直到下一个音不是单音，或不合条件
        //先添加第一个音
        seq.notes.Add(noteList[index] as SingleNote);
        while (hasNextNote(index) && noteList[index + 1] is SingleNote) {
            index++;    //将音符遍历循环中所使用的索引+1
            var lastNote = noteList[index - 1] as SingleNote;
            var nowNote = noteList[index] as SingleNote;
            switch (type) {
                case NoteSequence.Type.INCREASE:
                    if (nowNote.note > lastNote.note) {
                        seq.notes.Add(nowNote);
                    } else {
                        index--;    //光标始终指向可添加进序列的最后一个音
                        return seq;
                    }
                    break;
                case NoteSequence.Type.DECREASE:
                    if (nowNote.note < lastNote.note) {
                        seq.notes.Add(nowNote);
                    } else {
                        index--;    //光标始终指向可添加进序列的最后一个音
                        return seq;
                    }
                    break;
                case NoteSequence.Type.SAME_NOTE:
                    if (nowNote.note == lastNote.note) {
                        seq.notes.Add(nowNote);
                    } else {
                        index--;    //光标始终指向可添加进序列的最后一个音
                        return seq;
                    }
                    break;
                default:
                    throw new Exception("提供的音符序列类型不正确");
            }
        }
        return seq;
    }

    /// <summary>
    /// 将DataFrame中的音符装载进入列表中
    /// </summary>
    /// <returns></returns>
    public List<Note> loadNotes() {
        DataRowCollection rows = originalTable.Rows;
        //表格对多声部的表示有较大问题（详见含多声部的表格），需另做解析
        //修复表格中的多声部
        fixMultiVoicesInTable(rows);
        //遍历表格
        for (var i = 0; i < rows.Count; i++) {
            //将当前行封装为单音（不会将行中记录的指法封装到对象中，指法将自行计算）
            SingleNote note = rowToNote(rows[i]);
            //判断此行与之后若干行是否表示了一个复音
            //所有要获取下一行的操作必须先判断是否越界
            if (i + 1 >= rows.Count) {
                noteList.Add(note);
                return noteList;
            }
            //若下一行的起始帧大于或等于此行起始帧，且小于此行结束帧，则下一行与本行同属于一个复音
            SingleNote nextNote = rowToNote(rows[i + 1]);
            //判断是否需要封装复音
            if (nextNote.onFrame >= note.onFrame && nextNote.onFrame < note.offFrame) {
                //下一行与本行属于同一个复音
                var chord = new Chord(note);
                do {
                    //光标移动到下一行
                    i++;
                    //将移动后的行封装为单音，添加进复音对象中
                    //那么若是一个声部有一个很长的保持音，另一个声部有很多很短且有重复的单音，这也被算在复音中，应如何计算这种超大复音的指法？
                    //复音中不同的音高的单音数达到5个，就不再将后面的音加入复音，继续按原逻辑处理
                    SingleNote nowNote = rowToNote(rows[i]);
                    List<int> differentNotes = chord.differentNotes;
                    //若复音中不同音不少于5个，且这些不同音当中不包含移动后的行所表示的音符号
                    if(differentNotes.Count >= 5 && !differentNotes.Contains(nowNote.note))
                        break;    //不再将移动后的行及后面的音加入复音
                    else 
                        chord.add(nowNote);
                    //将之后的各行依次与移动前的第一行进行比较，直到某行的下一行不再与移动前的第一行同属于一个复音
                    if (i + 1 >= rows.Count) break;
                } while (rowToNote(rows[i + 1]).onFrame >= note.onFrame && 
                    rowToNote(rows[i + 1]).onFrame < note.offFrame);
                //将复音添加到列表
                noteList.Add(chord);
            } else {
                //本行与下一行不属于一个复音
                noteList.Add(note);
            }
        }
        //为每个音符设定编号
        for (var i = 0; i < noteList.Count; i++) {
            noteList[i].index = i;
        }
        return noteList;
    }

    /// <summary>
    /// 修复表格中的多声部
    /// </summary>
    /// <param name="rows"></param>
    public static void fixMultiVoicesInTable(DataRowCollection rows) {
        //定义获取行数据的方法
        int parse(string head, int index) {
            return int.Parse(rows[index][head].ToString());
        }
        //更新终止帧
        for (var i = 0; i < rows.Count; i++) {
            //查找表格中指法为0的行
            if (parse(" FINGER", i) != 0) continue;
            //记录本行信息
            int noteNo = parse(" NOTE", i);
            int offFrame = parse(" OFF_FRAME", i);
            //从此行的上一行开始，倒序依次查找
            for (int j = i - 1; j >= 0; j--) {
                //若找到某行与记录的音符号相同，且不为延长音，则将记录的音符号更新到此行
                if (parse(" NOTE", j) == noteNo && parse(" FINGER", j) != 0) {
                    rows[j][" OFF_FRAME"] = offFrame;
                    //只修改一个，修改后即不再继续向前查找
                    break;
                }
            }
        }
        //移除指法为0的行
        for (int i = rows.Count - 1; i >= 0; i--) {
            if(parse(" FINGER", i) == 0)
                rows.RemoveAt(i);
        }
    }

    /// <summary>
    /// 将数据表的一行封装为单音
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public SingleNote rowToNote(DataRow row) {
        //获取行数据
        int parse(string head) {
            return int.Parse(row[head].ToString());
        }
        int on = parse("#ON_FRAME");
        int off = parse(" OFF_FRAME");
        int note = parse(" NOTE");
        //封装为单音
        return new SingleNote(on, off, note);
    }

    /// <summary>
    /// 给定一个索引值，判断对于该索引值而言是否还有下一个音符
    /// </summary>
    /// <param name="nowIndex"></param>
    /// <returns></returns>
    private bool hasNextNote(int nowIndex) {
        return nowIndex + 1 < noteList.Count;
    }
    
    //用于计算弹奏每个音时需使用的五指位置，与手腕位置
    private HandOfCalculation hand = new HandOfCalculation();

    //用于按时间顺序存储音符
    private readonly List<Note> noteList = new List<Note>();
    
    public FingeringCalculator(Hand hand, DataTable table) {
        //prefix = hand.prefix;
        prefix = hand?.prefix;
        originalTable = table;
    }
    
    //前缀，用于判断左右手
    private readonly string prefix;

    //提供的原始音符帧数据表
    private readonly DataTable originalTable;
}

/// <summary>
/// 音符类的基类，其子类可作为音符使用
/// </summary>
public abstract class Note {
    
    //演奏此音时，五指与手腕所在位置
    public HandOfCalculation handPos;

    //音符在列表中的索引号
    public int index;
    
    //演奏类型
    public enum PlayType {
        JUMP, TURN
    }

    public PlayType type;
}

/// <summary>
/// 单音
/// </summary>
public class SingleNote : Note {
    
    public int onFrame, offFrame, note, finger = -1;

    public SingleNote(int onFrame, int offFrame, int note) {
        this.onFrame = onFrame;
        this.offFrame = offFrame;
        this.note = note;
    }

    public bool isBlackKey => MovingData.isBlackKey(note);

    /*//如果此对象大于传入的对象，返回0，反之则返回1
    public int CompareTo(object obj) {
        var sn = (SingleNote) obj;
        if (sn == null) return 0;
        return note > sn.note ? 0 : 1;
    }*/

    /// <summary>
    /// 计算此音到另一个音的音程
    /// </summary>
    /// <param name="sn"></param>
    /// <returns></returns>
    public int getNoteDistance(SingleNote sn) {
        return Math.Abs(sn.note - note);
    }

    public int getNoteDistance(int n) {
        return Math.Abs(n - note);
    }
}

/// <summary>
/// 复音（柱式和弦、琶音、波音）
/// </summary>
public class Chord : Note {
    
    /*//平均音高
    public int pitch => (int) differentNotes.Average();*/
    
    //最高音
    public SingleNote maxNote => notes[notes.Count - 1];
    
    //最低音
    public SingleNote minNote => notes[0];

    //最高音高
    public int maxPitch => differentNotes.Max();

    //最低音高
    public int minPitch => differentNotes.Min();
    
    //获取此复音中不同音高的音符集合
    public List<int> differentNotes {
        get {
            var diffNotes = new List<int>();
            foreach (SingleNote sn in notes) {
                if(!diffNotes.Contains(sn.note)) diffNotes.Add(sn.note);
            }
            return diffNotes;
        }
    }

    public void add(SingleNote sn) {
        notes.Add(sn);
        order();
    }
    
    public Chord(params SingleNote[] _notes) {
        foreach (SingleNote sn in _notes) {
            notes.Add(sn);
        }
        order();
    }

    private void order() {
        //列表中每一个成员用n表示，将列表根据n的音符号升序排序
        notes = notes.OrderBy(n => n.note).ToList();
    }

    public List<SingleNote> notes = new List<SingleNote>();
}

/// <summary>
/// 单音音符序列
/// </summary>
public class NoteSequence {
    
    //为序列中的音计算指法时是否需要考虑转指
    public bool shouldTurn = true;
    
    public readonly List<SingleNote> notes = new List<SingleNote>();

    public NoteSequence(Type type) {
        this.type = type;
    }

    public readonly Type type;

    public enum Type {
        INCREASE, DECREASE, SAME_NOTE
    }
}

/// <summary>
/// 用于记录和计算演奏某个音时五指位置与手掌位置
/// </summary>
public class HandOfCalculation {

    /// <summary>
    /// 手指，用于存储当前运动状态
    /// </summary>
    public class Finger {

        //当前所在音符
        public int nowNote;

        //当前所弹奏的音符的结束帧
        //处于此帧时，手指应该完全抬起
        public int offFrame = -1;

        //弹奏
        public void press(SingleNote sn) {
            nowNote = sn.note;
            offFrame = sn.offFrame;
        }

        //判断此手指在某帧时是否正在被使用
        public bool isInUse(int frameNo) {
            return frameNo < offFrame;
        }
    }

    public Finger[] fingers = new Finger[6];

    //手腕所在位置
    public int wristPos;

    /// <summary>
    /// 手指到某个音符的距离
    /// </summary>
    public struct FingerToNoteDistance {

        public int finger, note, distance;
    }

    /// <summary>
    /// 提供一个音符号，按顺序排列当前离此音符号最近的手指
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public List<FingerToNoteDistance> getNearestFingers(int note) {
        var list = new List<FingerToNoteDistance>();
        for (var i = 1; i <= 5; i++) {
            var ftnd = new FingerToNoteDistance {
                note = note,
                finger = i,
                distance = Math.Abs(note - fingers[i].nowNote)
            };
            list.Add(ftnd);
        }
        return list.OrderBy(n => n.distance).ToList();
    }

    public int getNearestFinger(int note) {
        return getNearestFingers(note)[0].finger;
    }

    /// <summary>
    /// 弹下一个音符，并根据当前五指位置自动排列未使用的手指，确定手腕位置，和弹奏类型
    /// </summary>
    /// <param name="note"></param>
    public void press(Note note) {
        //判断是以初始化方式弹奏还是结合现有手型弹奏
        if (!inited) {
            init(note);
        } else {
            switch (note) {
                case SingleNote _:
                    break;
                case Chord _:
                    break;
            }
        }
        //执行完成后，将现有手型信息复制到音符对象的成员中归档，以便在指法更改后重新计算手型
        note.handPos = clone();
    }

    private bool inited = false;

    /// <summary>
    /// 初始化五指的位置
    /// </summary>
    /// <param name="note"></param>
    private void init(Note note) {
        switch (note) {
            case SingleNote _:
                break;
            case Chord _:
                break;
        }
        inited = true;
    }

    /// <summary>
    /// 复制当前的五指与手腕位置
    /// </summary>
    /// <returns></returns>
    private HandOfCalculation clone() {
        //先执行浅复制
        var newObj = MemberwiseClone() as HandOfCalculation;
        //再将对象中每个引用类型的成员进行复制
        newObj.fingers = fingers.Clone() as Finger[];
        return newObj;
    }
}

}
```