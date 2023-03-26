using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using PianoPlayingMotionGenerator.HandModel;
using PianoPlayingMotionGenerator.Util;
using PianoPlayingMotionGenerator.Util.FingeringCalculator;

namespace DllTest {

/// <summary>
/// 在控制台进行的测试，内容通过控制台输出
/// </summary>
[TestFixture]
public class ConsoleTest {

    [Test]
    public void calcTest() {
        const string path = "xxxxx.csv";
        DataTable dt = CsvFileHelper.openCsv(path);
        var fc = new FingeringCalculator("右", dt);
        fc.loadNotes();
        List<Note> noteList = fc.calculateWithoutSeq();
        Out.println("索引\t音高\t\t指法\t\t类型");
        string typeToString(Note.PlayType type) {
            switch (type) {
                case Note.PlayType.NORMAL:
                    return "普通";
                case Note.PlayType.JUMP:
                    return "跳跃";
                case Note.PlayType.TURN:
                    return "转指";
                default:
                    return null;
            }
        }
        string getChordNote(Chord c) {
            var r = "";
            List<int> notes = new List<int>();
            foreach (SingleNote sn in c.notes) {
                if (!notes.Contains(sn.note)) {
                    r += sn.note + "-";
                    notes.Add(sn.note);
                }
            }
            return r;
        }
        string getChordFinger(Chord c) {
            var r = "";
            List<int> notes = new List<int>();
            foreach (SingleNote sn in c.notes) {
                if (!notes.Contains(sn.note)) {
                    r += sn.finger + "-";
                    notes.Add(sn.note);
                }
            }
            return r;
        }
        foreach (Note note in noteList) {
            switch (note) {
                case SingleNote sn:
                    Out.println($"{sn.index}\t\t{sn.note}\t\t\t{sn.finger}\t\t" +
                        $"{typeToString(sn.type)}");
                    break;
                case Chord chord:
                    Out.println($"{chord.index}\t\t{getChordNote(chord)}\t\t" +
                        $"{getChordFinger(chord)}\t{typeToString(chord.type)}");
                    break;
            }
        }
    }

    void fixTableTest() {
        const string path = "xxxxx.csv";
        DataTable dt = CsvFileHelper.openCsv(path);
        DataRowCollection rows = dt.Rows;
        FingeringCalculator.fixMultiVoicesInTable(rows);
        Out.println("ON_FRAME\tOFF_FRAME\tNOTE\tFINGER\tWRIST_POS");
        foreach (DataRow row in rows) {
            /*println("ON_FRAME：" + row["#ON_FRAME"] + "\t" +
                "OFF_FRAME：" + row[" OFF_FRAME"] + "\t" + 
                "NOTE：" + row[" NOTE"] + "\t" +
                "FINGER：" + row[" FINGER"] + "\t" +
                "WRIST_POS：" + row[" WRIST_POS"]);*/
            Out.println($"{row["#ON_FRAME"]}\t\t{row[" OFF_FRAME"]}\t\t" +
                $"{row[" NOTE"]}\t{row[" FINGER"]}\t{row[" WRIST_POS"]}");
        }
    }

    // void getNoteSequenceTest() {
    //     const string path = "xxxxx.csv";
    //     DataTable dt = CSVFileHelper.OpenCSV(path);
    //     var fc = new FingeringCalculator(null, dt);
    //     List<Note> noteList = fc.loadNotes();
    //     //遍历音符列表
    //     bool hasNextNote(int nowIndex) {
    //         return nowIndex + 1 < noteList.Count;
    //     }
    //     for (var i = 0; i < noteList.Count; i++) {
    //         //判断当前音符类型
    //         switch (noteList[i]) {
    //             //单音
    //             case SingleNote _:
    //                 var sn = noteList[i] as SingleNote;
    //                 //如果没有下一个音，或下一个音不是单音
    //                 if (!hasNextNote(i) || !(noteList[i + 1] is SingleNote)) {
    //                     println("单音：" + sn.note);
    //                     if(!hasNextNote(i)) return;
    //                     else continue;
    //                 }
    //                 //有下一个音，且下一个音是单音
    //                 var nextNote = noteList[i + 1] as SingleNote;
    //                 NoteSequence ns;
    //                 if (nextNote.note > sn.note) {
    //                     //下一个音符号大于本音符号，则应当处理为递增音符序列
    //                     ns = fc.getNoteSequence(ref i, NoteSequence.Type.INCREASE);
    //                     print("递增：");
    //                     foreach (SingleNote n in ns.notes) {
    //                         print(n.note + "-");
    //                     }
    //                     println();
    //                 } else if (nextNote.note < sn.note) {
    //                     ns = fc.getNoteSequence(ref i, NoteSequence.Type.DECREASE);
    //                     print("递减：");
    //                     foreach (SingleNote n in ns.notes) {
    //                         print(n.note + "-");
    //                     }
    //                     println();
    //                 } else if (nextNote.note == sn.note) {
    //                     ns = fc.getNoteSequence(ref i, NoteSequence.Type.SAME_NOTE);
    //                     print("同音：");
    //                     foreach (SingleNote n in ns.notes) {
    //                         print(n.note + "-");
    //                     }
    //                     println();
    //                 }
    //                 break;
    //             //复音
    //             case Chord _:
    //                 var chord = noteList[i] as Chord;
    //                 print("复音：");
    //                 foreach (int dNote in chord.differentNotes) {
    //                     print(dNote + "-");
    //                 }
    //                 println();
    //                 break;
    //         }
    //     }
    // }

    void loadNotesTest() {
        const string path = "xxxxx.csv";
        DataTable dt = CsvFileHelper.openCsv(path);
        var fc = new FingeringCalculator(null as Hand, dt);
        List<Note> noteList = fc.loadNotes();
        foreach (Note n in noteList) {
            if (n is SingleNote) {
                Out.print(((SingleNote) n).note + ", ");
            }
            if (n is Chord) {
                var c = (Chord) n;
                foreach (SingleNote sn in c.notes) {
                    Out.print(sn.note + "-");
                }
                Out.print(", ");
            }
        }
    }
}

public static class Out {
    
    public static void println(object o) {
        Console.WriteLine(o.ToString());
    }
    
    public static void println() {
        Console.WriteLine();
    }

    public static void print(object o) {
        Console.Write(o.ToString());
    }
}

}