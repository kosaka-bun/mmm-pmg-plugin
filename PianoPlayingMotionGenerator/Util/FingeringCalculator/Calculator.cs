using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PianoPlayingMotionGenerator.HandModel;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable LocalFunctionCanBeMadeStatic
// ReSharper disable ConvertSwitchStatementToSwitchExpression

namespace PianoPlayingMotionGenerator.Util.FingeringCalculator {

/// <summary>
/// 提供音符列表信息，为某只手计算用于此列表的合适的指法
/// </summary>
public partial class FingeringCalculator {

    /// <summary>
    /// 计算指法（不使用序列）
    /// </summary>
    /// <returns></returns>
    public List<Note> calculateWithoutSeq() {
        switch(prefix) {
            case "左":
                foreach(Note nowNote in noteList) {
                    switch(nowNote) {
                        case SingleNote sn:
                            if(nowNote.index == 0) calcLeftJumpToNote(sn);
                            else if(nowNote.index == noteList.Count - 1)
                                calcLeftLastNote(sn);
                            else calcLeftMidNote(sn);
                            break;
                        case Chord chord:
                            if(nowNote.index == 0) calcLeftJumpToChord(chord);
                            else calcLeftChord(chord);
                            break;
                    }
                }
                break;
            case "右":
                foreach(Note nowNote in noteList) {
                    switch(nowNote) {
                        case SingleNote sn:
                            if(nowNote.index == 0) calcJumpToNote(sn);
                            else if(nowNote.index == noteList.Count - 1)
                                calcLastNote(sn);
                            else calcMidNote(sn);
                            break;
                        case Chord chord:
                            if(nowNote.index == 0) calcJumpToChord(chord);
                            else calcChord(chord);
                            break;
                    }
                }
                break;
        }
        calcType();
        return noteList;
    }

    /// <summary>
    /// 计算指法（使用序列进行优化），返回计算好指法的Note列表
    /// </summary>
    /// <returns></returns>
    public List<Note> calculate() {
        //遍历音符列表
        for(var i = 0; i < noteList.Count; i++) {
            //判断当前音符类型
            switch(noteList[i]) {
                //单音
                case SingleNote _:
                    var sn = noteList[i] as SingleNote;
                    //如果没有下一个音，或下一个音不是单音
                    if(!hasNextNote(i) || !(noteList[i + 1] is SingleNote)) {
                        //TODO 寻找离此音最近的手指，安排指法
                        if(!hasNextNote(i)) return noteList;
                        else continue;
                    }
                    //有下一个音，且下一个音是单音
                    var nextNote = noteList[i + 1] as SingleNote;
                    //根据下一个音的音高，封装合适的音符序列
                    NoteSequence sequence = null;
                    if(nextNote.note > sn.note) {
                        //下一个音符号大于本音符号，则应当处理为递增音符序列
                        //递增序列
                        sequence = getNoteSequence(ref i, NoteSequence.Type.INCREASE);
                    } else if(nextNote.note < sn.note) {
                        //递减序列
                        sequence = getNoteSequence(ref i, NoteSequence.Type.DECREASE);
                    } else if(nextNote.note == sn.note) {
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
    /// 计算音符列表中每个音的演奏类型
    /// </summary>
    private void calcType() {
        switch(prefix) {
            case "左":
                foreach(Note now in noteList) {
                    if(now.index == 0) {
                        now.type = Note.PlayType.NORMAL;
                        continue;
                    }
                    Note prev = noteList[now.index - 1];
                    //前一个音为单音
                    if(prev is SingleNote prevNote) {
                        //后一个音为单音
                        if(now is SingleNote nowNote) {
                            //前一个音与后一个音的音程大于大九度
                            if(prevNote.getNoteDistance(nowNote) > Interval.BIG9) {
                                nowNote.type = Note.PlayType.JUMP;
                                continue;
                            }
                            //若前一个音低于后一个音，但前一个音的指法【小】于后一个音
                            //且当前音没有被标记（不得将已被记为跳跃的音记为转指）
                            if(prevNote.note < nowNote.note &&
                                prevNote.finger < nowNote.finger &&
                                nowNote.type == Note.PlayType.NORMAL) {
                                nowNote.type = Note.PlayType.TURN;
                                continue;
                            }
                            //若前一个音高于后一个音，且前一个音【不】使用1指，后一个音使用【1】指
                            if(prevNote.note > nowNote.note &&
                                prevNote.finger != 1 &&
                                nowNote.finger == 1 &&
                                nowNote.type == Note.PlayType.NORMAL) {
                                nowNote.type = Note.PlayType.TURN;
                                continue;
                            }
                        }
                        //后一个音为复音
                        else if(now is Chord nowChord) {
                            //后一个音包含前一个音用过的手指
                            if(nowChord.fingers.Contains(prevNote.finger)) {
                                nowChord.type = Note.PlayType.JUMP;
                                continue;
                            }
                            //最低音或最高音与前一个音音程超过八度
                            if(prevNote.getNoteDistance(nowChord.minPitch) >
                                Interval.PERFECT8 ||
                                prevNote.getNoteDistance(nowChord.maxPitch) >
                                Interval.PERFECT8) {
                                nowChord.type = Note.PlayType.JUMP;
                                continue;
                            }
                            //前一个音的音高介于复音的最低音与最高音之间，且前一个音用1指，后一个音没有用1指
                            if(prevNote.finger == 1 &&
                                !nowChord.fingers.Contains(1) &&
                                prevNote.note > nowChord.minPitch &&
                                prevNote.note < nowChord.maxPitch &&
                                nowChord.type == Note.PlayType.NORMAL) {
                                nowChord.type = Note.PlayType.TURN;
                                continue;
                            }
                            //前一个音【高】于复音最【高】音，但不使用1指，而复音使用了1指
                            if(prevNote.note > nowChord.maxPitch &&
                                prevNote.finger != 1 &&
                                nowChord.fingers.Contains(1) &&
                                nowChord.type == Note.PlayType.NORMAL) {
                                nowChord.type = Note.PlayType.TURN;
                                continue;
                            }
                        }
                    }
                    //前一个音为复音
                    else if(prev is Chord prevChord) {
                        //后一个音为单音
                        if(now is SingleNote nowNote) {
                            //后一个音是前一个音用过的手指
                            if(prevChord.fingers.Contains(nowNote.finger)) {
                                nowNote.type = Note.PlayType.JUMP;
                                continue;
                            }
                            //当前音距前一个音的最低音或最高音音程超过八度
                            if(nowNote.getNoteDistance(prevChord.minPitch) >
                                Interval.PERFECT8 ||
                                nowNote.getNoteDistance(prevChord.maxPitch) >
                                Interval.PERFECT8) {
                                nowNote.type = Note.PlayType.JUMP;
                                continue;
                            }
                            //当前音介于前一个音的最低音与最高音之间，且当前音使用1指，前一个音不使用1指
                            if(nowNote.note > prevChord.minPitch &&
                                nowNote.note < prevChord.maxPitch &&
                                nowNote.finger == 1 &&
                                !prevChord.fingers.Contains(1) &&
                                nowNote.type == Note.PlayType.NORMAL) {
                                nowNote.type = Note.PlayType.TURN;
                                continue;
                            }
                            //前一个复音使用1指，当前音不使用1指，且【高】于复音的最【高】音
                            if(prevChord.fingers.Contains(1) &&
                                nowNote.finger != 1 &&
                                nowNote.note > prevChord.maxPitch &&
                                nowNote.type == Note.PlayType.NORMAL) {
                                nowNote.type = Note.PlayType.TURN;
                                continue;
                            }
                        }
                        //后一个音为复音
                        else if(now is Chord nowChord) {
                            //默认是被记为跳跃的，不记为跳跃的连续复音通常也没办法转指
                            //故不作处理
                        }
                    }
                }
                break;
            case "右":
                foreach(Note now in noteList) {
                    if(now.index == 0) {
                        now.type = Note.PlayType.NORMAL;
                        continue;
                    }
                    Note prev = noteList[now.index - 1];
                    //前一个音为单音
                    if(prev is SingleNote prevNote) {
                        //后一个音为单音
                        if(now is SingleNote nowNote) {
                            //前一个音与后一个音的音程大于大九度
                            if(prevNote.getNoteDistance(nowNote) > Interval.BIG9) {
                                nowNote.type = Note.PlayType.JUMP;
                                continue;
                            }
                            //若前一个音低于后一个音，但前一个音的指法大于后一个音
                            //且当前音没有被标记（不得将已被记为跳跃的音记为转指）
                            if(prevNote.note < nowNote.note &&
                                prevNote.finger > nowNote.finger &&
                                nowNote.type == Note.PlayType.NORMAL) {
                                nowNote.type = Note.PlayType.TURN;
                                continue;
                            }
                            //若前一个音高于后一个音，且前一个音使用1指，后一个音使用其他指
                            if(prevNote.note > nowNote.note &&
                                prevNote.finger == 1 &&
                                nowNote.finger > prevNote.finger &&
                                nowNote.type == Note.PlayType.NORMAL) {
                                nowNote.type = Note.PlayType.TURN;
                                continue;
                            }
                        }
                        //后一个音为复音
                        else if(now is Chord nowChord) {
                            //后一个音包含前一个音用过的手指
                            if(nowChord.fingers.Contains(prevNote.finger)) {
                                nowChord.type = Note.PlayType.JUMP;
                                continue;
                            }
                            //最低音或最高音与前一个音音程超过八度
                            if(prevNote.getNoteDistance(nowChord.minPitch) >
                                Interval.PERFECT8 ||
                                prevNote.getNoteDistance(nowChord.maxPitch) >
                                Interval.PERFECT8) {
                                nowChord.type = Note.PlayType.JUMP;
                                continue;
                            }
                            //前一个音的音高介于复音的最低音与最高音之间，且前一个音用1指，后一个音没有用1指
                            if(prevNote.finger == 1 &&
                                !nowChord.fingers.Contains(1) &&
                                prevNote.note > nowChord.minPitch &&
                                prevNote.note < nowChord.maxPitch &&
                                nowChord.type == Note.PlayType.NORMAL) {
                                nowChord.type = Note.PlayType.TURN;
                                continue;
                            }
                            //前一个音低于复音最低音，但不使用1指，而复音使用了1指
                            if(prevNote.note < nowChord.minPitch &&
                                prevNote.finger != 1 &&
                                nowChord.fingers.Contains(1) &&
                                nowChord.type == Note.PlayType.NORMAL) {
                                nowChord.type = Note.PlayType.TURN;
                                continue;
                            }
                        }
                    }
                    //前一个音为复音
                    else if(prev is Chord prevChord) {
                        //后一个音为单音
                        if(now is SingleNote nowNote) {
                            //后一个音是前一个音用过的手指
                            if(prevChord.fingers.Contains(nowNote.finger)) {
                                nowNote.type = Note.PlayType.JUMP;
                                continue;
                            }
                            //当前音距前一个音的最低音或最高音音程超过八度
                            if(nowNote.getNoteDistance(prevChord.minPitch) >
                                Interval.PERFECT8 ||
                                nowNote.getNoteDistance(prevChord.maxPitch) >
                                Interval.PERFECT8) {
                                nowNote.type = Note.PlayType.JUMP;
                                continue;
                            }
                            //当前音介于前一个音的最低音与最高音之间，且当前音使用1指，前一个音不使用1指
                            if(nowNote.note > prevChord.minPitch &&
                                nowNote.note < prevChord.maxPitch &&
                                nowNote.finger == 1 &&
                                !prevChord.fingers.Contains(1) &&
                                nowNote.type == Note.PlayType.NORMAL) {
                                nowNote.type = Note.PlayType.TURN;
                                continue;
                            }
                            //前一个复音使用1指，当前音不使用1指，且低于复音的最低音
                            if(prevChord.fingers.Contains(1) &&
                                nowNote.finger != 1 &&
                                nowNote.note < prevChord.minPitch &&
                                nowNote.type == Note.PlayType.NORMAL) {
                                nowNote.type = Note.PlayType.TURN;
                                continue;
                            }
                        }
                        //后一个音为复音
                        else if(now is Chord nowChord) {
                            //默认是被记为跳跃的，不记为跳跃的连续复音通常也没办法转指
                            //故不作处理
                        }
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 根据手的前缀，来计算一个音符序列的指法
    /// </summary>
    /// <param name="seq"></param>
    public void calculateSequence(NoteSequence seq) {
        switch(prefix) {
            case "左":
                switch(seq.type) {
                    case NoteSequence.Type.INCREASE:
                        break;
                    case NoteSequence.Type.DECREASE:
                        break;
                    case NoteSequence.Type.SAME_NOTE:
                        break;
                }
                break;
            case "右":
                switch(seq.type) {
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
    /// 计算右手上行序列
    /// </summary>
    /// <param name="seq"></param>
    private void calcIncreaseSeq(NoteSequence seq) {
    }

    /// <summary>
    /// 计算右手下行序列
    /// </summary>
    /// <param name="seq"></param>
    private void calcDecreaseSeq(NoteSequence seq) {
    }

    /// <summary>
    /// 计算右手同音序列
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
        while(hasNextNote(index) && noteList[index + 1] is SingleNote) {
            index++; //将音符遍历循环中所使用的索引+1
            var lastNote = noteList[index - 1] as SingleNote;
            var nowNote = noteList[index] as SingleNote;
            switch(type) {
                case NoteSequence.Type.INCREASE:
                    if(nowNote.note > lastNote.note) {
                        seq.notes.Add(nowNote);
                    } else {
                        index--; //光标始终指向可添加进序列的最后一个音
                        return seq;
                    }
                    break;
                case NoteSequence.Type.DECREASE:
                    if(nowNote.note < lastNote.note) {
                        seq.notes.Add(nowNote);
                    } else {
                        index--; //光标始终指向可添加进序列的最后一个音
                        return seq;
                    }
                    break;
                case NoteSequence.Type.SAME_NOTE:
                    if(nowNote.note == lastNote.note) {
                        seq.notes.Add(nowNote);
                    } else {
                        index--; //光标始终指向可添加进序列的最后一个音
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
        for(var i = 0; i < rows.Count; i++) {
            //将当前行封装为单音（不会将行中记录的指法封装到对象中，指法将自行计算）
            SingleNote note = rowToNote(rows[i]);
            //判断此行与之后若干行是否表示了一个复音
            //所有要获取下一行的操作必须先判断是否越界
            if(i + 1 >= rows.Count) {
                noteList.Add(note);
                break;
            }
            //若下一行的起始帧大于或等于此行起始帧，且小于此行结束帧，则下一行与本行同属于一个复音
            SingleNote nextNote = rowToNote(rows[i + 1]);
            //判断是否需要封装复音
            if(nextNote.onFrame >= note.onFrame && nextNote.onFrame < note.offFrame) {
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
                        break; //不再将移动后的行及后面的音加入复音
                    else
                        chord.add(nowNote);
                    //将之后的各行依次与移动前的第一行进行比较，直到某行的下一行不再与移动前的第一行同属于一个复音
                    if(i + 1 >= rows.Count) break;
                } while(rowToNote(rows[i + 1]).onFrame >= note.onFrame &&
                    rowToNote(rows[i + 1]).onFrame < note.offFrame);
                //将复音添加到列表
                noteList.Add(chord);
            } else {
                //本行与下一行不属于一个复音
                noteList.Add(note);
            }
        }
        //为每个音符设定编号
        for(var i = 0; i < noteList.Count; i++) {
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
        for(var i = 0; i < rows.Count; i++) {
            //查找表格中指法为0的行
            if(parse(" FINGER", i) != 0) continue;
            //记录本行信息
            int noteNo = parse(" NOTE", i);
            int offFrame = parse(" OFF_FRAME", i);
            //从此行的上一行开始，倒序依次查找
            for(int j = i - 1; j >= 0; j--) {
                //若找到某行与记录的音符号相同，且不为延长音，则将记录的音符号更新到此行
                if(parse(" NOTE", j) == noteNo && parse(" FINGER", j) != 0) {
                    rows[j][" OFF_FRAME"] = offFrame;
                    //只修改一个，修改后即不再继续向前查找
                    break;
                }
            }
        }
        //移除指法为0的行
        for(int i = rows.Count - 1; i >= 0; i--) {
            if(parse(" FINGER", i) == 0)
                rows.RemoveAt(i);
        }
    }

    /// <summary>
    /// 将数据表的一行封装为单音
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private SingleNote rowToNote(DataRow row) {
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

    /*//用于计算弹奏每个音时需使用的五指位置，与手腕位置
    private HandOfCalculation hand = new HandOfCalculation();*/

    //用于按时间顺序存储音符
    private readonly List<Note> noteList = new List<Note>();

    public FingeringCalculator(Hand hand, DataTable table)
        : this(hand.prefix, table) {
    }

    public FingeringCalculator(string _prefix, DataTable table) {
        prefix = _prefix;
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

    //音符在列表中的索引号
    public int index;

    //音符的起始帧（或最小起始帧）
    public int onFrame {
        get {
            switch(this) {
                case SingleNote note:
                    return note.onFrame;
                case Chord chord:
                    return chord.notesInOnFrameOrder[0].onFrame;
                default:
                    throw new Exception("音符是未知类型");
            }
        }
    }

    //音符的结束帧（或最大结束帧）
    public int offFrame {
        get {
            switch(this) {
                case SingleNote note:
                    return note.offFrame;
                case Chord chord:
                    List<SingleNote> notesInOffFrameOrder = chord.notes.OrderByDescending(n => n.offFrame).ToList();
                    return notesInOffFrameOrder[0].offFrame;
                default:
                    throw new Exception("音符是未知类型");
            }
        }
    }
    
    //所用到的手指
    public int[] fingers {
        get {
            switch(this) {
                case SingleNote sn:
                    return new[] { sn.finger };
                case Chord chord:
                    return chord.fingers.ToArray();
            }
            return null;
        }
    }

    //演奏类型
    public enum PlayType {
        NORMAL,
        JUMP,
        TURN
    }

    public PlayType type = PlayType.NORMAL;
    
    private static string typeToString(PlayType _type) {
        switch(_type) {
            case PlayType.NORMAL:
                return "普通";
            case PlayType.JUMP:
                return "跳跃";
            case PlayType.TURN:
                return "转指";
            default:
                return "";
        }
    }

    private static string getChordNote(Chord c) {
        var r = "";
        var notes = new List<int>();
        foreach(SingleNote sn in c.notes) {
            if(!notes.Contains(sn.note)) {
                r += sn.note + "-";
                notes.Add(sn.note);
            }
        }
        return r;
    }

    private static string getChordFinger(Chord c) {
        var r = "";
        var notes = new List<int>();
        foreach(SingleNote sn in c.notes) {
            if(!notes.Contains(sn.note)) {
                r += sn.finger + "-";
                notes.Add(sn.note);
            }
        }
        return r;
    }

    public override string ToString() {
        switch(this) {
            case SingleNote note:
                return $"索引：{note.index}，音高：{note.note}，" +
                    $"指法：{note.finger}，类型：{typeToString(note.type)}";
            case Chord chord:
                return $"索引：{chord.index}，音高：{getChordNote(chord)}，" +
                    $"指法：{getChordFinger(chord)}，" +
                    $"类型：{typeToString(chord.type)}";
            default:
                return base.ToString();
        }
    }

    public bool containsFinger(int finger) {
        switch(this) {
            case SingleNote sn:
                return finger == sn.finger;
            case Chord chord:
                return chord.fingers.Contains(finger);
            default:
                return false;
        }
    }
}

/// <summary>
/// 单音
/// </summary>
public class SingleNote : Note {

    public new int onFrame, offFrame;

    public int note, finger = -1;

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

    //所用的指法
    public List<int> fingers {
        get {
            var _fingers = new List<int>();
            foreach(SingleNote sn in notes) {
                if(!_fingers.Contains(sn.finger))
                    _fingers.Add(sn.finger);
            }
            return _fingers;
        }
    }

    //最高音
    public SingleNote maxNote => notes[notes.Count - 1];

    //最低音
    public SingleNote minNote => notes[0];

    //最高音高
    public int maxPitch => differentNotes.Max();

    //最低音高
    public int minPitch => differentNotes.Min();

    /// <summary>
    /// 最大起始帧
    /// </summary>
    public int maxOnFrame {
        get {
            List<int> onFrames = notes.Select(note => note.onFrame).ToList();
            return onFrames.Max();
        }
    }

    //获取此复音中不同音高的音符集合
    public List<int> differentNotes {
        get {
            var diffNotes = new List<int>();
            foreach(SingleNote sn in notes) {
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
        foreach(SingleNote sn in _notes) {
            notes.Add(sn);
        }
        order();
    }

    private void order() {
        //列表中每一个成员用n表示，将列表根据n的音符号升序排序
        notes = notes.OrderBy(n => n.note).ToList();
    }

    public List<SingleNote> notes = new List<SingleNote>();

    public List<SingleNote> notesInOnFrameOrder {
        get { return notes.OrderBy(n => n.onFrame).ToList(); }
    }
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
        INCREASE,
        DECREASE,
        SAME_NOTE
    }
}

/*/// <summary>
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
*/

}