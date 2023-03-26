using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable IdentifierTypo
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable MergeCastWithTypeCheck
// ReSharper disable InconsistentNaming
// ReSharper disable ReturnTypeCanBeEnumerable.Local
// ReSharper disable ConvertIfStatementToReturnStatement

namespace PianoPlayingMotionGenerator.Util.FingeringCalculator {

/// <summary>
/// 指法计算器，核心算法部分
/// </summary>
public partial class FingeringCalculator {

    /// <summary>
    /// 基于右手，根据一个复音的前一个音（需确保存在），计算此复音的指法
    /// （不考虑后一个音）
    /// </summary>
    /// <param name="chord"></param>
    private void calcChord(Chord chord) {
        Note _prev = noteList[chord.index - 1];
        if(_prev is SingleNote) {
            var prev = _prev as SingleNote;
            //判断前一个音的音高
            //低于复音的最低音
            if(prev.note <= chord.minPitch) {
                //能否直接到达复音的最高音
                List<int> toMaxNote = calcHigherByLower(prev, chord.maxNote);
                //若能，找到第一个高于前一个音指法的推荐指法，然后从它开始，倒序计算复音的指法
                if(toMaxNote.Count > 0) {
                    int toMaxNoteFinger = -1;
                    foreach(int recFinger in toMaxNote) {
                        if(recFinger > prev.finger) {
                            toMaxNoteFinger = recFinger;
                            break;
                        }
                    }
                    //存在这样的指法，且有效手指个数大于或等于复音中不同音高的音符数
                    if(toMaxNoteFinger != -1 &&
                        toMaxNoteFinger >= chord.differentNotes.Count) {
                        //从高到低计算复音指法，且最高音使用推荐指法，然后依次使用较小的指法
                        calcChordWithNothing(chord, false, toMaxNoteFinger);
                        return;
                    }
                    //若不存在，则跳跃到达此复音
                    calcJumpToChord(chord);
                    return;
                }
                //若不能，则跳跃到达此复音
                calcJumpToChord(chord);
            }
            //高于复音的最高音
            else if(prev.note >= chord.maxPitch) {
                //能否直接到达复音的最低音
                List<int> toMinNote = calcLowerByHigher(prev, chord.minNote);
                //若能，找到第一个【低】于前一个音指法的推荐指法，然后从它开始，【正序】计算
                if(toMinNote.Count > 0) {
                    int toMinNoteFinger = -1;
                    foreach(int recFinger in toMinNote) {
                        if(recFinger < prev.finger) {
                            toMinNoteFinger = recFinger;
                            break;
                        }
                    }
                    //存在这样的指法，且有效手指个数大于或等于复音中不同音高的音符数
                    if(toMinNoteFinger != -1 &&
                        (5 - toMinNoteFinger + 1) >= chord.differentNotes.Count) {
                        //【从低到高】计算复音指法，最低音使用推荐指法
                        calcChordWithNothing(chord, true, toMinNoteFinger);
                        return;
                    }
                    //若不存在，则跳跃到达此复音
                    calcJumpToChord(chord);
                    return;
                }
                //若不能，则跳跃到达此复音
                calcJumpToChord(chord);
            }
            //介于最低音与最高音之间
            else {
                //直接视为需要跳跃到达的音，但是在计算指法后需要先将演奏类型置为普通，
                //待之后判断
                calcJumpToChord(chord);
                chord.type = Note.PlayType.NORMAL;
                //若前一个音使用的指法在复音中没有出现，且介于最高音指法与最低音指法之间，
                //则可以不用跳跃
            }
        } else if(_prev is Chord) {
            var prev = _prev as Chord;
            //判断两个复音当中是否某个复音有三个及以上的不同音高，任意一个有，则跳跃
            if(prev.differentNotes.Count >= 3 || chord.differentNotes.Count >= 3) {
                calcJumpToChord(chord);
                return;
            }
            //两个复音均为双音复音
            //判断两个复音是否有交叉（其中一个复音的最高音介于另一个复音的最高音与
            //最低音之间，最低音小于另一个复音的最低音）
            //前一个双音较低
            if(prev.minPitch < chord.minPitch && prev.maxPitch > chord.minPitch &&
                prev.maxPitch < chord.maxPitch) {
                //获得前一个双音最低音到后一个双音最低音的推荐指法
                List<int> minToMin = calcHigherByLower(prev.minNote, chord.minNote);
                //若没有推荐指法，则跳跃
                if(minToMin.Count <= 0) {
                    calcJumpToChord(chord);
                    return;
                }
                foreach(int minToMinFinger in minToMin) {
                    //建立无关音符，计算使用此指法的后一个双音的最低音
                    //到后一个双音最高音的推荐指法
                    var nextMinNote =
                        new SingleNote(0, 0, chord.minPitch);
                    nextMinNote.finger = minToMinFinger;
                    List<int> minToMax =
                        calcHigherByLower(nextMinNote, chord.maxNote);
                    //如果没有推荐指法，则进行下一次循环
                    if(minToMax.Count <= 0) continue;
                    //若有，则最低音采用此指法，最高音采用推荐指法中高于最低音指法的第一个
                    foreach(int minToMaxFinger in minToMax) {
                        if(minToMaxFinger > minToMinFinger) {
                            foreach(SingleNote sn in chord.notes) {
                                if(sn.note == chord.minPitch)
                                    sn.finger = minToMinFinger;
                                else if(sn.note == chord.maxPitch)
                                    sn.finger = minToMaxFinger;
                                else
                                    throw new Exception("双音中出现了" +
                                        "不同于最低音和最高音的音符");
                            }
                            return;
                        }
                    }
                    //若最高音的推荐指法中，没有大于最低音所使用的指法的，则继续下一次循环
                }
                //若遍历了最低音可用的推荐指法之后，最低音仍未分配指法，则跳跃
                if(chord.minNote.finger == -1) {
                    calcJumpToChord(chord);
                    return;
                }
            }
            //后一个双音较低
            else if(chord.minPitch < prev.minPitch && 
                chord.maxPitch > prev.minPitch &&
                chord.maxPitch < prev.maxPitch) {
                //此代码复制而来
                //获得前一个双音最低音到后一个双音最低音的推荐指法
                List<int> minToMin = calcLowerByHigher(prev.minNote, chord.minNote);
                //若没有推荐指法，则跳跃
                if(minToMin.Count <= 0) {
                    calcJumpToChord(chord);
                    return;
                }
                foreach(int minToMinFinger in minToMin) {
                    //建立无关音符，计算使用此指法的后一个双音的最低音，
                    //到后一个双音最高音的推荐指法
                    var nextMinNote =
                        new SingleNote(0, 0, chord.minPitch);
                    nextMinNote.finger = minToMinFinger;
                    List<int> minToMax =
                        calcHigherByLower(nextMinNote, chord.maxNote);
                    //如果没有推荐指法，则进行下一次循环
                    if(minToMax.Count <= 0) continue;
                    //若有，则最低音采用此指法，最高音采用推荐指法中高于最低音指法的第一个
                    foreach(int minToMaxFinger in minToMax) {
                        if(minToMaxFinger > minToMinFinger) {
                            foreach(SingleNote sn in chord.notes) {
                                if(sn.note == chord.minPitch)
                                    sn.finger = minToMinFinger;
                                else if(sn.note == chord.maxPitch)
                                    sn.finger = minToMaxFinger;
                                else
                                    throw new Exception("双音中出现了" +
                                        "不同于最低音和最高音的音符");
                            }
                            return;
                        }
                    }
                    //若最高音的推荐指法中，没有大于最低音所使用的指法的，则继续下一次循环
                }
                //若遍历了最低音可用的推荐指法之后，最低音仍未分配指法，则跳跃
                if(chord.minNote.finger == -1) {
                    calcJumpToChord(chord);
                    return;
                }
            }
            //两个双音没有交叉（这样的音型非常少见）
            else {
                //判断哪个双音更高
                //若后一个双音最低音大于前一个双音最高音，则后一个双音较高
                if(chord.minPitch > prev.maxPitch) {
                    //判断较低双音的最高音能否用5指到达较高双音的最高音，若不能，则跳跃
                    List<int> maxToMax = calcHigherByLower(prev.maxNote, 
                        chord.maxNote);
                    if(maxToMax.Count <= 0 || !maxToMax.Contains(5)) {
                        calcJumpToChord(chord);
                        return;
                    }
                    calcChordWithNothing(chord, false, 5);
                    /*//判断后一个双音最高音是否是黑键
                    if (chord.maxNote.isBlackKey) {
                        calcChordWithNothing(chord, false, 4);
                    } else {
                        calcChordWithNothing(chord, false, 5);
                    }*/
                } else if(prev.minPitch > chord.maxPitch) {
                    //判断较高双音的最低音能否用1指到达较低双音的最低音
                    List<int> minToMin = calcLowerByHigher(prev.minNote, 
                        chord.minNote);
                    if(minToMin.Count <= 0 || !minToMin.Contains(1)) {
                        calcJumpToChord(chord);
                        return;
                    }
                    calcChordWithNothing(chord, true, 1);
                    /*if (chord.minNote.isBlackKey) {
                        calcChordWithNothing(chord, true, 2);
                    } else {
                        calcChordWithNothing(chord, true, 1);
                    }*/
                } else {
                    calcJumpToChord(chord);
                }
            }
        }
    }

    private void calcLeftChord(Chord chord) {
        //复制而来
        Note _prev = noteList[chord.index - 1];
        if(_prev is SingleNote) {
            var prev = _prev as SingleNote;
            //判断前一个音的音高
            //低于复音的最低音
            if(prev.note <= chord.minPitch) {
                //能否直接到达复音的最高音
                List<int> toMaxNote = calcLeftHigherByLower(prev, chord.maxNote);
                //若能，找到第一个【低】于前一个音指法的推荐指法，然后从它开始，
                //倒序计算复音的指法
                if(toMaxNote.Count > 0) {
                    int toMaxNoteFinger = -1;
                    foreach(int recFinger in toMaxNote) {
                        if(recFinger < prev.finger) {
                            toMaxNoteFinger = recFinger;
                            break;
                        }
                    }
                    //存在这样的指法，且有效手指个数大于或等于复音中不同音高的音符数
                    if(toMaxNoteFinger != -1 &&
                        (5 - toMaxNoteFinger + 1) >= chord.differentNotes.Count) {
                        //从高到低计算复音指法，且最高音使用推荐指法，然后依次使用较小的指法
                        calcLeftChordWithNothing(chord,
                            false, toMaxNoteFinger);
                        return;
                    }
                    //若不存在，则跳跃到达此复音
                    calcLeftJumpToChord(chord);
                    return;
                }
                //若不能，则跳跃到达此复音
                calcLeftJumpToChord(chord);
            }
            //高于复音的最高音
            else if(prev.note >= chord.maxPitch) {
                //能否直接到达复音的最低音
                List<int> toMinNote = calcLeftLowerByHigher(prev, chord.minNote);
                //若能，找到第一个【高】于前一个音指法的推荐指法，然后从它开始，【正序】计算
                if(toMinNote.Count > 0) {
                    int toMinNoteFinger = -1;
                    foreach(int recFinger in toMinNote) {
                        if(recFinger > prev.finger) {
                            toMinNoteFinger = recFinger;
                            break;
                        }
                    }
                    //存在这样的指法，且有效手指个数大于或等于复音中不同音高的音符数
                    if(toMinNoteFinger != -1 &&
                        toMinNoteFinger >= chord.differentNotes.Count) {
                        //【从低到高】计算复音指法，最低音使用推荐指法
                        calcLeftChordWithNothing(chord,
                            true, toMinNoteFinger);
                        return;
                    }
                    //若不存在，则跳跃到达此复音
                    calcLeftJumpToChord(chord);
                    return;
                }
                //若不能，则跳跃到达此复音
                calcLeftJumpToChord(chord);
            }
            //介于最低音与最高音之间
            else {
                //直接视为需要跳跃到达的音，但是在计算指法后需要先将演奏类型置为普通，
                //待之后判断
                calcLeftJumpToChord(chord);
                chord.type = Note.PlayType.NORMAL;
                //若前一个音使用的指法在复音中没有出现，且介于最高音指法与最低音指法之间，
                //则可以不用跳跃
            }
        } else if(_prev is Chord) {
            var prev = _prev as Chord;
            //判断两个复音当中是否某个复音有三个及以上的不同音高，任意一个有，则跳跃
            if(prev.differentNotes.Count >= 3 || chord.differentNotes.Count >= 3) {
                calcLeftJumpToChord(chord);
                return;
            }
            //两个复音均为双音复音
            //判断两个复音是否有交叉（其中一个复音的最高音介于另一个复音的最高音与
            //最低音之间，最低音小于另一个复音的最低音）
            //前一个双音较低
            if(prev.minPitch < chord.minPitch && prev.maxPitch > chord.minPitch &&
                prev.maxPitch < chord.maxPitch) {
                //获得前一个双音最低音到后一个双音最低音的推荐指法
                List<int> minToMin = calcLeftHigherByLower(prev.minNote, 
                    chord.minNote);
                //若没有推荐指法，则跳跃
                if(minToMin.Count <= 0) {
                    calcLeftJumpToChord(chord);
                    return;
                }
                foreach(int minToMinFinger in minToMin) {
                    //建立无关音符，计算使用此指法的后一个双音的最低音，
                    //到后一个双音最高音的推荐指法
                    var nextMinNote =
                        new SingleNote(0, 0, chord.minPitch);
                    nextMinNote.finger = minToMinFinger;
                    List<int> minToMax =
                        calcLeftHigherByLower(nextMinNote, chord.maxNote);
                    //如果没有推荐指法，则进行下一次循环
                    if(minToMax.Count <= 0) continue;
                    //若有，则最低音采用此指法，最高音采用推荐指法中【低】于最低音指法的第一个
                    foreach(int minToMaxFinger in minToMax) {
                        if(minToMaxFinger < minToMinFinger) {
                            foreach(SingleNote sn in chord.notes) {
                                if(sn.note == chord.minPitch)
                                    sn.finger = minToMinFinger;
                                else if(sn.note == chord.maxPitch)
                                    sn.finger = minToMaxFinger;
                                else
                                    throw new Exception("双音中出现了" +
                                        "不同于最低音和最高音的音符");
                            }
                            return;
                        }
                    }
                    //若最高音的推荐指法中，没有大于最低音所使用的指法的，则继续下一次循环
                }
                //若遍历了最低音可用的推荐指法之后，最低音仍未分配指法，则跳跃
                if(chord.minNote.finger == -1) {
                    calcLeftJumpToChord(chord);
                    return;
                }
            }
            //后一个双音较低
            else if(chord.minPitch < prev.minPitch && 
                chord.maxPitch > prev.minPitch &&
                chord.maxPitch < prev.maxPitch) {
                //此代码复制而来
                //获得前一个双音最低音到后一个双音最低音的推荐指法
                List<int> minToMin = calcLeftLowerByHigher(prev.minNote, 
                    chord.minNote);
                //若没有推荐指法，则跳跃
                if(minToMin.Count <= 0) {
                    calcLeftJumpToChord(chord);
                    return;
                }
                foreach(int minToMinFinger in minToMin) {
                    //建立无关音符，计算使用此指法的后一个双音的最低音，
                    //到后一个双音最高音的推荐指法
                    var nextMinNote =
                        new SingleNote(0, 0, chord.minPitch);
                    nextMinNote.finger = minToMinFinger;
                    List<int> minToMax =
                        calcLeftHigherByLower(nextMinNote, chord.maxNote);
                    //如果没有推荐指法，则进行下一次循环
                    if(minToMax.Count <= 0) continue;
                    //若有，则最低音采用此指法，最高音采用推荐指法中【低】于最低音指法的第一个
                    foreach(int minToMaxFinger in minToMax) {
                        if(minToMaxFinger < minToMinFinger) {
                            foreach(SingleNote sn in chord.notes) {
                                if(sn.note == chord.minPitch)
                                    sn.finger = minToMinFinger;
                                else if(sn.note == chord.maxPitch)
                                    sn.finger = minToMaxFinger;
                                else
                                    throw new Exception("双音中出现了" +
                                        "不同于最低音和最高音的音符");
                            }
                            return;
                        }
                    }
                    //若最高音的推荐指法中，没有大于最低音所使用的指法的，则继续下一次循环
                }
                //若遍历了最低音可用的推荐指法之后，最低音仍未分配指法，则跳跃
                if(chord.minNote.finger == -1) {
                    calcLeftJumpToChord(chord);
                    return;
                }
            }
            //两个双音没有交叉（这样的音型非常少见）
            else {
                //判断哪个双音更高
                //若后一个双音最低音大于前一个双音最高音，则后一个双音较高
                if(chord.minPitch > prev.maxPitch) {
                    //判断较低双音的最高音能否使用1指到达较高双音的最高音，若不能，则跳跃
                    List<int> maxToMax =
                        calcLeftHigherByLower(prev.maxNote, chord.maxNote);
                    if(maxToMax.Count <= 0 || !maxToMax.Contains(1)) {
                        calcLeftJumpToChord(chord);
                        return;
                    }
                    calcLeftChordWithNothing(chord, false, 1);
                } else if(prev.minPitch > chord.maxPitch) {
                    //判断较高双音的最低音能否用5指到达较低双音的最低音
                    List<int> minToMin =
                        calcLeftLowerByHigher(prev.minNote, chord.minNote);
                    if(minToMin.Count <= 0 || !minToMin.Contains(5)) {
                        calcLeftJumpToChord(chord);
                        return;
                    }
                    calcLeftChordWithNothing(chord, true, 5);
                } else {
                    calcLeftJumpToChord(chord);
                }
            }
        }
    }

    /// <summary>
    /// 基于右手，根据它的后一个音（如果有）计算需要跳跃到达的复音的指法
    /// </summary>
    /// <param name="chord"></param>
    private void calcJumpToChord(Chord chord) {
        chord.type = Note.PlayType.JUMP;
        //是否是列表中最后一个音
        if(chord.index == noteList.Count - 1) {
            //判断前一个音
            switch(noteList[chord.index - 1]) {
                case SingleNote prevNote:
                    //判断音高
                    if(prevNote.note < chord.minPitch) {
                        calcChordWithNothing(chord, false, 5);
                    } else if(prevNote.note > chord.maxPitch) {
                        calcChordWithNothing(chord, true, 1);
                    } else {
                        if(prevNote.getNoteDistance(chord.minPitch) <=
                            prevNote.getNoteDistance(chord.maxPitch)) {
                            calcChordWithNothing(chord, true, 1);
                        } else {
                            calcChordWithNothing(chord, false, 5);
                        }
                    }
                    break;
                case Chord prevChord:
                    if(prevChord.maxPitch <= chord.maxPitch) {
                        calcChordWithNothing(chord, false, 5);
                    } else {
                        calcChordWithNothing(chord, true, 1);
                    }
                    break;
            }
            return;
        }
        //不是最后一个音
        switch(noteList[chord.index + 1]) {
            case SingleNote nextNote:
                if(chord.minPitch <= nextNote.note) {
                    calcChordWithNothing(chord, true, 1);
                } else {
                    calcChordWithNothing(chord, false, 5);
                }
                break;
            case Chord nextChord:
                if(chord.minPitch <= nextChord.minPitch) {
                    calcChordWithNothing(chord, true, 1);
                } else {
                    if(chord.maxPitch < nextChord.maxPitch &&
                        chord.differentNotes.Count <= 4) {
                        calcChordWithNothing(chord, false, 4);
                    } else {
                        calcChordWithNothing(chord, false, 5);
                    }
                }
                break;
        }
    }

    private void calcLeftJumpToChord(Chord chord) {
        //复制而来
        chord.type = Note.PlayType.JUMP;
        //是否是列表中最后一个音
        if(chord.index == noteList.Count - 1) {
            //判断前一个音
            switch(noteList[chord.index - 1]) {
                case SingleNote prevNote:
                    //判断音高
                    if(prevNote.note < chord.minPitch) {
                        calcLeftChordWithNothing(chord, false, 1);
                    } else if(prevNote.note > chord.maxPitch) {
                        calcLeftChordWithNothing(chord, true, 5);
                    } else {
                        if(prevNote.getNoteDistance(chord.minPitch) <=
                            prevNote.getNoteDistance(chord.maxPitch)) {
                            calcLeftChordWithNothing(chord, 
                                true, 5);
                        } else {
                            calcLeftChordWithNothing(chord, 
                                false, 1);
                        }
                    }
                    break;
                case Chord prevChord:
                    if(prevChord.maxPitch <= chord.maxPitch) {
                        calcLeftChordWithNothing(chord, false, 1);
                    } else {
                        calcLeftChordWithNothing(chord, true, 5);
                    }
                    break;
            }
            return;
        }
        //不是最后一个音
        switch(noteList[chord.index + 1]) {
            case SingleNote nextNote:
                if(chord.minPitch <= nextNote.note) {
                    calcLeftChordWithNothing(chord, true, 5);
                } else {
                    calcLeftChordWithNothing(chord, false, 1);
                }
                break;
            case Chord nextChord:
                if(chord.minPitch <= nextChord.minPitch) {
                    calcLeftChordWithNothing(chord, true, 5);
                } else {
                    if(chord.maxPitch < nextChord.maxPitch &&
                        chord.differentNotes.Count <= 4) {
                        calcLeftChordWithNothing(chord, false, 2);
                    } else {
                        calcLeftChordWithNothing(chord, false, 1);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 基于右手，不考虑前后音，直接根据音符遍历方向计算复音的指法
    /// </summary>
    /// <param name="chord"></param>
    private void calcChordWithNothing(Chord chord, bool lowToHigh, 
        int startFinger) {
        //当前正在分配中的手指号
        int finger = startFinger;
        //剩余不同音的个数
        int remainNotes = chord.differentNotes.Count;
        if(lowToHigh) {
            //从起始手指开始，从低到高遍历复音中的音符，依次为每个音赋予指法
            for(var i = 0; i < chord.notes.Count; i++) {
                //复音中每个单音都是按音高从低到高排列的
                SingleNote note = chord.notes[i];
                //是否是复音中第一个音，若是，则直接赋予起始手指
                if(i == 0) {
                    note.finger = finger;
                    continue;
                }
                //不是第一个音
                SingleNote prev = chord.notes[i - 1];
                //判断和前一个音是否音高相同，若是，和前一个音使用同一指法
                if(note.note == prev.note) {
                    note.finger = prev.finger;
                    continue;
                }
                //若不是，表示前面若干个相同音高的音的指法计算完成，它们均使用同一个手指
                //剩余的不同音高的音符数减少
                remainNotes--;
                //剩余可用手指数
                int remainFingers = 5 - finger;
                //若二者刚好相等，则可将剩余的手指依次对应到剩余的音符上
                if(remainNotes == remainFingers) {
                    finger++; //移动到下一个可用手指
                    note.finger = finger; //分配
                }
                //若剩余手指较多，则可以选取推荐指法
                else if(remainFingers > remainNotes) {
                    //计算从复音中前一个音（已分配指法，且音高低于当前音）到当前音的推荐指法
                    List<int> recommendFingers = calcHigherByLower(prev, note);
                    //遍历推荐指法
                    foreach(int recFinger in recommendFingers) {
                        //若此指法低于或等于上一个目前已分配的手指，则跳过
                        if(recFinger <= finger) continue;
                        //TODO 若此指法属于避免使用的指法（如上一个音所使用的指法），则跳过
                        //若此音尝试使用此指法后，会导致剩余手指少于剩余的不同音符数
                        //（即分配指法前所剩余的不同音符数-1），则不采用
                        int remainFingersAfter = 5 - recFinger;
                        if(remainFingersAfter < remainNotes - 1) continue;
                        //若不会导致，则采用
                        finger = recFinger;
                        note.finger = finger;
                        break;
                    }
                    //若没有可用的推荐指法，此音指法将不会被分配，应根据剩余音符数予以分配
                    //TODO 若没有可用的推荐指法，则应忽略避免使用的指法，
                    //再进行一次遍历，仍没有推荐指法，再按剩余音符数分配
                    if(note.finger == -1) {
                        //使得分配以后，剩余手指数等于剩余音数
                        finger = 5 - remainNotes + 1;
                        note.finger = finger;
                    }
                }
                //若剩余手指不足，则提供的参数有误，或算法出现了错误
                else {
                    //TODO 此算法在遇到极端跨度复音（通常是多声部），如C5保持，
                    //然后依次演奏C4、#C4，D4时，并不能正确计算指法
                    throw new Exception("计算复音指法时，剩余手指数少于" +
                        "了不同音符中还未分配指法的音符数，请检查提供的参数，" +
                        "或算法是否正确。");
                }
            }
        }
        //从高到低，倒序遍历音符列表，倒序分配手指
        else {
            //此代码由上方代码复制而来，作简单修改，若上方修改，此处应重写
            for(int i = chord.notes.Count - 1; i >= 0; i--) {
                SingleNote note = chord.notes[i];
                //是否是倒数第一个音
                if(i == chord.notes.Count - 1) {
                    note.finger = finger;
                    continue;
                }
                //不是倒数第一个音
                SingleNote next = chord.notes[i + 1];
                //判断和后一个音是否音高相同，若是，和后一个音使用同一指法
                if(note.note == next.note) {
                    note.finger = next.finger;
                    continue;
                }
                //若不是，表示后面若干个相同音高的音的指法计算完成，它们均使用同一个手指
                //剩余的不同音高的音符数减少
                remainNotes--;
                //剩余可用手指数
                int remainFingers = finger - 1;
                //若二者刚好相等，则可将剩余的手指依次对应到剩余的音符上
                if(remainNotes == remainFingers) {
                    finger--; //移动到下一个可用手指
                    note.finger = finger; //分配
                }
                //若剩余手指较多，则可以选取推荐指法
                else if(remainFingers > remainNotes) {
                    //计算推荐指法
                    List<int> recommendFingers = calcLowerByHigher(next, note);
                    //遍历推荐指法
                    foreach(int recFinger in recommendFingers) {
                        //若此指法【高】于或等于上一个目前已分配的手指，则跳过
                        if(recFinger >= finger) continue;
                        //若此音尝试使用此指法后，会导致剩余手指少于剩余的不同音符数
                        //（即分配指法前所剩余的不同音符数-1），则不采用
                        int remainFingersAfter = recFinger - 1;
                        if(remainFingersAfter < remainNotes - 1) continue;
                        //若不会导致，则采用
                        finger = recFinger;
                        note.finger = finger;
                        break;
                    }
                    //若没有推荐指法，此音指法将不会被分配，应选择后一个手指予以分配
                    if(note.finger == -1) {
                        finger = remainNotes;
                        note.finger = finger;
                    }
                }
                //若剩余手指不足，则提供的参数有误，或算法出现了错误
                else {
                    throw new Exception("计算复音指法时，剩余手指数少于了" +
                        "不同音符中还未分配指法的音符数，请检查提供的参数，" +
                        "或算法是否正确。");
                }
            }
        }
    }

    private void calcLeftChordWithNothing(Chord chord, bool lowToHigh, 
        int startFinger) {
        //复制而来
        //当前正在分配中的手指号
        int finger = startFinger;
        //剩余不同音的个数
        int remainNotes = chord.differentNotes.Count;
        if(lowToHigh) {
            //从起始手指开始，从低到高遍历复音中的音符，依次为每个音赋予指法
            for(var i = 0; i < chord.notes.Count; i++) {
                //复音中每个单音都是按音高从低到高排列的
                SingleNote note = chord.notes[i];
                //是否是复音中第一个音，若是，则直接赋予起始手指
                if(i == 0) {
                    note.finger = finger;
                    continue;
                }
                //不是第一个音
                SingleNote prev = chord.notes[i - 1];
                //判断和前一个音是否音高相同，若是，和前一个音使用同一指法
                if(note.note == prev.note) {
                    note.finger = prev.finger;
                    continue;
                }
                //若不是，表示前面若干个相同音高的音的指法计算完成，它们均使用同一个手指
                //剩余的不同音高的音符数减少
                remainNotes--;
                //剩余可用手指数【左手】
                int remainFingers = finger - 1;
                //若二者刚好相等，则可将剩余的手指依次对应到剩余的音符上
                if(remainNotes == remainFingers) {
                    finger--; //移动到下一个可用手指【左手】
                    note.finger = finger; //分配
                }
                //若剩余手指较多，则可以选取推荐指法
                else if(remainFingers > remainNotes) {
                    //计算从复音中前一个音（已分配指法，且音高低于当前音）到当前音的推荐指法
                    List<int> recommendFingers = calcLeftHigherByLower(prev, note);
                    //遍历推荐指法
                    foreach(int recFinger in recommendFingers) {
                        //若此指法高于或等于上一个目前已分配的手指，则跳过
                        if(recFinger >= finger) continue;
                        //若此音尝试使用此指法后，会导致剩余手指少于剩余的不同音符数
                        //（即分配指法前所剩余的不同音符数-1），则不采用
                        int remainFingersAfter = recFinger - 1;
                        if(remainFingersAfter < remainNotes - 1) continue;
                        //若不会导致，则采用
                        finger = recFinger;
                        note.finger = finger;
                        break;
                    }
                    //若没有推荐指法，此音指法将不会被分配，应选择后一个手指予以分配
                    if(note.finger == -1) {
                        finger = remainNotes;
                        note.finger = finger;
                    }
                }
                //若剩余手指不足，则提供的参数有误，或算法出现了错误
                else {
                    throw new Exception("计算复音指法时，剩余手指数少于了" +
                        "不同音符中还未分配指法的音符数，请检查提供的参数，" +
                        "或算法是否正确。");
                }
            }
        } else {
            for(int i = chord.notes.Count - 1; i >= 0; i--) {
                SingleNote note = chord.notes[i];
                //是否是倒数第一个音
                if(i == chord.notes.Count - 1) {
                    note.finger = finger;
                    continue;
                }
                //不是倒数第一个音
                SingleNote next = chord.notes[i + 1];
                //判断和后一个音是否音高相同，若是，和后一个音使用同一指法
                if(note.note == next.note) {
                    note.finger = next.finger;
                    continue;
                }
                //若不是，表示后面若干个相同音高的音的指法计算完成，它们均使用同一个手指
                //剩余的不同音高的音符数减少
                remainNotes--;
                //剩余可用手指数【左手】
                int remainFingers = 5 - finger;
                //若二者刚好相等，则可将剩余的手指依次对应到剩余的音符上
                if(remainNotes == remainFingers) {
                    finger++; //移动到下一个可用手指【左手】
                    note.finger = finger; //分配
                }
                //若剩余手指较多，则可以选取推荐指法
                else if(remainFingers > remainNotes) {
                    //计算推荐指法【左手】
                    List<int> recommendFingers = calcLeftLowerByHigher(next, note);
                    //遍历推荐指法
                    foreach(int recFinger in recommendFingers) {
                        //若此指法低于或等于上一个目前已分配的手指，则跳过
                        if(recFinger <= finger) continue;
                        //若此音尝试使用此指法后，会导致剩余手指少于剩余的不同音符数
                        //（即分配指法前所剩余的不同音符数-1），则不采用
                        int remainFingersAfter = 5 - recFinger;
                        if(remainFingersAfter < remainNotes - 1) continue;
                        //若不会导致，则采用
                        finger = recFinger;
                        note.finger = finger;
                        break;
                    }
                    //若没有推荐指法，此音指法将不会被分配，应选择后一个手指予以分配
                    if(note.finger == -1) {
                        finger = 5 - remainNotes + 1;
                        note.finger = finger;
                    }
                }
                //若剩余手指不足，则提供的参数有误，或算法出现了错误
                else {
                    throw new Exception("计算复音指法时，剩余手指数少于了" +
                        "不同音符中还未分配指法的音符数，请检查提供的参数，" +
                        "或算法是否正确。");
                }
            }
        }
    }

    /// <summary>
    /// 基于右手，当后一个音不存在，且本音为单音时，计算此音的指法
    /// </summary>
    /// <param name="note"></param>
    private void calcLastNote(SingleNote note) {
        //从计算中间音的方法中复制前半部分而来
        //前一个音
        Note _prev = noteList[note.index - 1];
        //列表中音的顺序代表优先级，越靠前越推荐
        List<int> availFinger = null;
        //判断前一个音的类型
        if(_prev is SingleNote) {
            var prev = _prev as SingleNote;
            //前一个音低于中间音
            if(prev.note < note.note) {
                availFinger = calcHigherByLower(prev, note);
            }
            //高于中间音
            else if(prev.note > note.note) {
                availFinger = calcLowerByHigher(prev, note);
            }
            //等于中间音
            else {
                availFinger = calcSameNote(prev);
            }
        } else if(_prev is Chord) {
            var prevChord = _prev as Chord;
            //中间音高于前一个复音的最高音，等同于根据最高音的单音判断后一个音的指法
            if(note.note > prevChord.maxPitch) {
                //创建一个无关音符，赋予复音中最高音的音高，与最高音所使用的指法
                var prevNote =
                    new SingleNote(0, 0, prevChord.maxPitch);
                prevNote.finger = prevChord.notes[prevChord.notes.Count - 1].finger;
                //交予方法判断
                availFinger = calcHigherByLower(prevNote, note);
            }
            //中间音低于前一个复音的最低音
            else if(note.note < prevChord.minPitch) {
                //创建一个无关音符复制复音中最低音的音高于指法
                var prevNote =
                    new SingleNote(0, 0, prevChord.minPitch);
                prevNote.finger = prevChord.notes[0].finger;
                //交予方法判断
                availFinger = calcLowerByHigher(prevNote, note);
            }
            //其他情况
            else {
                //判断离哪个音更近
                //离最低音更近
                if(note.getNoteDistance(prevChord.minPitch) <
                    note.getNoteDistance(prevChord.maxPitch)) {
                    //计算从前一个复音的最低音到此音的指法
                    availFinger = calcHigherByLower(prevChord.minNote, note);
                } else {
                    availFinger = calcLowerByHigher(prevChord.maxNote, note);
                }
            }
        }
        //推荐指法是否为空，若是，则应当跳跃到此音
        if(availFinger.Count <= 0) {
            calcJumpToNote(note);
        }
        //若不为空，则采用最推荐的指法
        else {
            note.finger = availFinger[0];
        }
    }

    private void calcLeftLastNote(SingleNote note) {
        //从计算中间音的方法中复制前半部分而来
        //前一个音
        Note _prev = noteList[note.index - 1];
        //列表中音的顺序代表优先级，越靠前越推荐
        List<int> availFinger = null;
        //判断前一个音的类型
        if(_prev is SingleNote) {
            var prev = _prev as SingleNote;
            //前一个音低于中间音
            if(prev.note < note.note) {
                availFinger = calcLeftHigherByLower(prev, note);
            }
            //高于中间音
            else if(prev.note > note.note) {
                availFinger = calcLeftLowerByHigher(prev, note);
            }
            //等于中间音
            else {
                availFinger = calcSameNote(prev);
            }
        } else if(_prev is Chord) {
            var prevChord = _prev as Chord;
            //中间音高于前一个复音的最高音，等同于根据最高音的单音判断后一个音的指法
            if(note.note > prevChord.maxPitch) {
                //创建一个无关音符，赋予复音中最高音的音高，与最高音所使用的指法
                var prevNote =
                    new SingleNote(0, 0, prevChord.maxPitch);
                prevNote.finger = prevChord.notes[prevChord.notes.Count - 1].finger;
                //交予方法判断
                availFinger = calcLeftHigherByLower(prevNote, note);
            }
            //中间音低于前一个复音的最低音
            else if(note.note < prevChord.minPitch) {
                //创建一个无关音符复制复音中最低音的音高于指法
                var prevNote =
                    new SingleNote(0, 0, prevChord.minPitch);
                prevNote.finger = prevChord.notes[0].finger;
                //交予方法判断
                availFinger = calcLeftLowerByHigher(prevNote, note);
            }
            //其他情况
            else {
                //判断离哪个音更近
                //离最低音更近
                if(note.getNoteDistance(prevChord.minPitch) <
                    note.getNoteDistance(prevChord.maxPitch)) {
                    //计算从前一个复音的最低音到此音的指法
                    availFinger = calcLeftHigherByLower(prevChord.minNote, note);
                } else {
                    availFinger = calcLeftLowerByHigher(prevChord.maxNote, note);
                }
            }
        }
        //推荐指法是否为空，若是，则应当跳跃到此音
        if(availFinger.Count <= 0) {
            calcLeftJumpToNote(note);
        }
        //若不为空，则采用最推荐的指法
        else {
            note.finger = availFinger[0];
        }
    }

    /// <summary>
    /// 基于右手，当前一个音与后一个音均存在，且中间音为单音时，计算中间单音的指法
    /// </summary>
    /// <param name="note">中间音</param>
    private void calcMidNote(SingleNote note) {
        //此时，前一个音已分配指法，中间音与后一个音均未分配指法
        //前一个音与后一个音
        Note _prev = noteList[note.index - 1], _next = noteList[note.index + 1];
        //前一个音到中间音时，中间音的可用指法
        //列表中音的顺序代表优先级，越靠前越推荐
        List<int> availFinger = null;
        //分别判断前一个音与后一个音的类型
        //先计算从前一个音到中间音时，中间音的可用指法
        if(_prev is SingleNote) {
            var prev = _prev as SingleNote;
            //前一个音低于中间音
            if(prev.note < note.note) {
                availFinger = calcHigherByLower(prev, note);
            }
            //高于中间音
            else if(prev.note > note.note) {
                availFinger = calcLowerByHigher(prev, note);
            }
            //等于中间音
            else {
                availFinger = calcSameNote(prev);
            }
        } else if(_prev is Chord) {
            var prevChord = _prev as Chord;
            //中间音高于前一个复音的最高音，等同于根据最高音的单音判断后一个音的指法
            if(note.note > prevChord.maxPitch) {
                //创建一个无关音符，赋予复音中最高音的音高，与最高音所使用的指法
                var prevNote =
                    new SingleNote(0, 0, prevChord.maxPitch);
                prevNote.finger = prevChord.notes[prevChord.notes.Count - 1].finger;
                //交予方法判断
                availFinger = calcHigherByLower(prevNote, note);
            }
            //中间音低于前一个复音的最低音
            else if(note.note < prevChord.minPitch) {
                //创建一个无关音符复制复音中最低音的音高于指法
                var prevNote =
                    new SingleNote(0, 0, prevChord.minPitch);
                prevNote.finger = prevChord.notes[0].finger;
                //交予方法判断
                availFinger = calcLowerByHigher(prevNote, note);
            }
            //其他情况
            else {
                //判断离哪个音更近
                //离最低音更近
                if(note.getNoteDistance(prevChord.minPitch) <
                    note.getNoteDistance(prevChord.maxPitch)) {
                    //计算从前一个复音的最低音到此音的指法
                    availFinger = calcHigherByLower(prevChord.minNote, note);
                } else {
                    availFinger = calcLowerByHigher(prevChord.maxNote, note);
                }
            }
        }
        //再根据给出的推荐指法，判断是否能到达后一个音
        //推荐指法是否为空，若是，则应当跳跃到此音
        if(availFinger.Count <= 0) {
            calcJumpToNote(note);
            return;
        }
        //不为空，则根据下一个音的音高于属性，判断选择哪个推荐指法
        if(_next is SingleNote) {
            var next = _next as SingleNote;
            //遍历每一个推荐的指法，看能否到达下一个音，如果能则采用
            foreach(int finger in availFinger) {
                //创建用于计算的、无关的中间音对象，分别使用每一个推荐指法
                var tmpMidNote = new SingleNote(0, 0, note.note);
                tmpMidNote.finger = finger;
                //中间音的音高低于后一个音
                if(tmpMidNote.note < next.note) {
                    //若中间音使用此指法，那么从它到达下一个音可用的推荐指法用此集合存储
                    List<int> toNext = calcHigherByLower(tmpMidNote, next);
                    //若能够到达下一个音，则采用此指法
                    if(toNext.Count > 0) {
                        note.finger = finger;
                        return;
                    }
                    //若不能，则继续判断下一个推荐指法
                }
                //高于后一个音
                else if(tmpMidNote.note > next.note) {
                    List<int> toNext = calcLowerByHigher(tmpMidNote, next);
                    if(toNext.Count > 0) {
                        note.finger = finger;
                        return;
                    }
                }
                //等于后一个音，采用第一个推荐指法
                else {
                    note.finger = availFinger[0];
                    return;
                }
            }
            //如果均不能到达，则从中间音到下一个音需要跳跃
            //下一个音高于中间音，采用推荐指法中最小的一个
            if(next.note > note.note) {
                note.finger = availFinger.Min();
            }
            //低于中间音，采用推荐指法中最大的一个
            else if(next.note < note.note) {
                note.finger = availFinger.Max();
            }
            //其他情况，采用推荐指法第一个
            else {
                note.finger = availFinger[0];
            }
        } else if(_next is Chord) {
            var nextChord = _next as Chord;
            //遍历推荐指法，看能否达到后一个复音的每一个音
            foreach(int finger in availFinger) {
                var tmpMidNote = new SingleNote(0, 0, note.note);
                tmpMidNote.finger = finger;
                //遍历复音中的每一个音，看中间音使用这个指法是否都能达到每一个音
                var canGet = true;
                foreach(SingleNote sn in nextChord.notes) {
                    List<int> toNext;
                    if(note.note < sn.note) {
                        toNext = calcHigherByLower(tmpMidNote, sn);
                    } else if(note.note > sn.note) {
                        toNext = calcLowerByHigher(tmpMidNote, sn);
                    } else {
                        toNext = calcSameNote(tmpMidNote);
                    }
                    //任意一个音不能到达，则使用当前指法不能到达下一个复音
                    if(toNext.Count <= 0) {
                        canGet = false;
                        break;
                    }
                }
                //若每个音都能到达，则采用此指法
                if(canGet) {
                    note.finger = finger;
                    return;
                }
            }
            //不能到达，则判断当前音的音高
            //当前音低于复音的最低音，则采用推荐指法中最小的一个
            if(note.note < nextChord.minPitch) {
                note.finger = availFinger.Min();
            } else if(note.note > nextChord.maxPitch) {
                note.finger = availFinger.Max();
            } else {
                note.finger = availFinger[0];
            }

            /*//中间音低于后一个复音的最低音
            if (note.note < nextChord.minPitch) {
                //遍历推荐指法，看能否达到后一个复音的每一个音
                foreach (int finger in availFinger) {
                    var tmpMidNote = new SingleNote(0, 0, note.note);
                    tmpMidNote.finger = finger;
                    //遍历复音中的每一个音，看中间音使用这个指法是否都能达到每一个音
                    var canGet = true;
                    foreach (SingleNote sn in nextChord.notes) {
                        List<int> toNext =
                            calcHigherByLower(tmpMidNote, sn);
                        //任意一个音不能到达，则使用当前指法不能到达下一个复音
                        if (toNext.Count <= 0) {
                            canGet = false;
                            break;
                        }
                    }
                    //若每个音都能到达，则采用此指法
                    if (canGet) {
                        note.finger = finger;
                        return;
                    }
                }
                //不能到达，采用推荐指法中最小的一个
                note.finger = availFinger.Min();
            }
            //高于后一个复音的最高音
            else if (note.note > nextChord.maxPitch) {
                //遍历推荐指法，看能否达到后一个复音的每一个音
                foreach (int finger in availFinger) {
                    var tmpMidNote = new SingleNote(0, 0, note.note);
                    tmpMidNote.finger = finger;
                    //遍历复音中的每一个音，看中间音使用这个指法是否都能达到每一个音
                    var canGet = true;
                    foreach (SingleNote sn in nextChord.notes) {
                        List<int> toNext =
                            calcLowerByHigher(tmpMidNote, sn);
                        //任意一个音不能到达，则使用当前指法不能到达下一个复音
                        if (toNext.Count <= 0) {
                            canGet = false;
                            break;
                        }
                    }
                    //若每个音都能到达，则采用此指法
                    if (canGet) {
                        note.finger = finger;
                        return;
                    }
                }
                //不能到达，采用推荐指法中最大的一个
                note.finger = availFinger.Max();
            }
            //其他情况
            else {
                
            }*/
        }

        //若处理后，音符的指法仍未计算，则采用第一个推荐指法
        if(note.finger == -1) {
            note.finger = availFinger[0];
        }

        //TODO 可以考虑在这里加上判断，判断中间音是否是转指到达的，然后更改中间音的演奏类型
    }

    private void calcLeftMidNote(SingleNote note) {
        //此时，前一个音已分配指法，中间音与后一个音均未分配指法
        //前一个音与后一个音
        Note _prev = noteList[note.index - 1], _next = noteList[note.index + 1];
        //前一个音到中间音时，中间音的可用指法
        //列表中音的顺序代表优先级，越靠前越推荐
        List<int> availFinger = null;
        //分别判断前一个音与后一个音的类型
        //先计算从前一个音到中间音时，中间音的可用指法
        if(_prev is SingleNote) {
            var prev = _prev as SingleNote;
            //前一个音低于中间音
            if(prev.note < note.note) {
                availFinger = calcLeftHigherByLower(prev, note);
            }
            //高于中间音
            else if(prev.note > note.note) {
                availFinger = calcLeftLowerByHigher(prev, note);
            }
            //等于中间音
            else {
                availFinger = calcSameNote(prev);
            }
        } else if(_prev is Chord) {
            var prevChord = _prev as Chord;
            //中间音高于前一个复音的最高音，等同于根据最高音的单音判断后一个音的指法
            if(note.note > prevChord.maxPitch) {
                //创建一个无关音符，赋予复音中最高音的音高，与最高音所使用的指法
                var prevNote =
                    new SingleNote(0, 0, prevChord.maxPitch);
                prevNote.finger = prevChord.notes[prevChord.notes.Count - 1].finger;
                //交予方法判断
                availFinger = calcLeftHigherByLower(prevNote, note);
            }
            //中间音低于前一个复音的最低音
            else if(note.note < prevChord.minPitch) {
                //创建一个无关音符复制复音中最低音的音高于指法
                var prevNote =
                    new SingleNote(0, 0, prevChord.minPitch);
                prevNote.finger = prevChord.notes[0].finger;
                //交予方法判断
                availFinger = calcLeftLowerByHigher(prevNote, note);
            }
            //其他情况
            else {
                //判断离哪个音更近
                //离最低音更近
                if(note.getNoteDistance(prevChord.minPitch) <
                    note.getNoteDistance(prevChord.maxPitch)) {
                    //计算从前一个复音的最低音到此音的指法
                    availFinger = calcLeftHigherByLower(prevChord.minNote, note);
                } else {
                    availFinger = calcLeftLowerByHigher(prevChord.maxNote, note);
                }
            }
        }
        //再根据给出的推荐指法，判断是否能到达后一个音
        //推荐指法是否为空，若是，则应当跳跃到此音
        if(availFinger.Count <= 0) {
            calcLeftJumpToNote(note);
            return;
        }
        //不为空
        if(_next is SingleNote) {
            var next = _next as SingleNote;
            //遍历每一个推荐的指法，看能否到达下一个音，如果能则采用
            foreach(int finger in availFinger) {
                //创建用于计算的、无关的中间音对象，分别使用每一个推荐指法
                var tmpMidNote = new SingleNote(0, 0, note.note);
                tmpMidNote.finger = finger;
                //中间音的音高低于后一个音
                if(tmpMidNote.note < next.note) {
                    //若中间音使用此指法，那么从它到达下一个音可用的推荐指法用此集合存储
                    List<int> toNext = calcLeftHigherByLower(tmpMidNote, 
                        next);
                    //若能够到达下一个音，则采用此指法
                    if(toNext.Count > 0) {
                        note.finger = finger;
                        return;
                    }
                    //若不能，则继续判断下一个推荐指法
                }
                //高于后一个音
                else if(tmpMidNote.note > next.note) {
                    List<int> toNext = calcLeftLowerByHigher(tmpMidNote, 
                        next);
                    if(toNext.Count > 0) {
                        note.finger = finger;
                        return;
                    }
                }
                //等于后一个音，采用第一个推荐指法
                else {
                    note.finger = availFinger[0];
                    return;
                }
            }
            //如果均不能到达，则从中间音到下一个音需要跳跃
            //下一个音高于中间音，采用推荐指法中最【大】的一个
            if(next.note > note.note) {
                note.finger = availFinger.Max();
            }
            //低于中间音，采用推荐指法中最【小】的一个
            else if(next.note < note.note) {
                note.finger = availFinger.Min();
            }
            //其他情况，采用推荐指法第一个
            else {
                note.finger = availFinger[0];
            }
        } else if(_next is Chord) {
            var nextChord = _next as Chord;
            //遍历推荐指法，看能否达到后一个复音的每一个音
            foreach(int finger in availFinger) {
                var tmpMidNote = new SingleNote(0, 0, note.note);
                tmpMidNote.finger = finger;
                //遍历复音中的每一个音，看中间音使用这个指法是否都能达到每一个音
                var canGet = true;
                foreach(SingleNote sn in nextChord.notes) {
                    List<int> toNext;
                    if(note.note < sn.note) {
                        toNext = calcLeftHigherByLower(tmpMidNote, sn);
                    } else if(note.note > sn.note) {
                        toNext = calcLeftLowerByHigher(tmpMidNote, sn);
                    } else {
                        toNext = calcSameNote(tmpMidNote);
                    }
                    //任意一个音不能到达，则使用当前指法不能到达下一个复音
                    if(toNext.Count <= 0) {
                        canGet = false;
                        break;
                    }
                }
                //若每个音都能到达，则采用此指法
                if(canGet) {
                    note.finger = finger;
                    return;
                }
            }
            //不能到达，则判断当前音的音高
            //当前音低于复音的最低音，则采用推荐指法中最【大】的一个
            if(note.note < nextChord.minPitch) {
                note.finger = availFinger.Max();
            } else if(note.note > nextChord.maxPitch) {
                note.finger = availFinger.Min();
            } else {
                note.finger = availFinger[0];
            }
        }

        //若处理后，音符的指法仍未计算，则采用第一个推荐指法
        if(note.finger == -1) {
            note.finger = availFinger[0];
        }
    }

    /// <summary>
    /// 基于右手，计算某个需要跳跃达到的单音的指法
    /// </summary>
    /// <param name="note"></param>
    private void calcJumpToNote(SingleNote note) {
        note.type = Note.PlayType.JUMP;
        //需要跳跃到达的音通常应忽略前一个音
        //若当前音为音符列表最后一个音，则应考虑前一个音
        if(note.index == noteList.Count - 1) {
            Note _prev = noteList[note.index - 1];
            if(_prev is SingleNote) {
                var prev = _prev as SingleNote;
                note.finger = prev.note < note.note ? 5 : 1;
            } else if(_prev is Chord) {
                var prev = _prev as Chord;
                if(note.note > prev.maxPitch)
                    note.finger = 5;
                else if(note.note < prev.minPitch)
                    note.finger = 1;
                else note.finger = 3;
            }
            return;
        }
        //若不是，则仅考虑后一个音即可
        Note _next = noteList[note.index + 1];
        //后一个音为单音
        if(_next is SingleNote) {
            var next = _next as SingleNote;
            //音高高于当前音
            if(next.note > note.note) {
                //当前音是否是黑键，若是，则应当考虑是否用2指
                if(note.isBlackKey) {
                    //若后一个音与此音的音程大于纯四度，用1指
                    if(note.getNoteDistance(next) > Interval.PERFECT4) {
                        note.finger = 1;
                    }
                    //等于纯四度且为黑键，用1指
                    else if(note.getNoteDistance(next) == Interval.PERFECT4 &&
                        next.isBlackKey) {
                        note.finger = 1;
                    }
                    //小于纯四度
                    //通常情况下，用2指
                    else {
                        note.finger = 2;
                    }
                }
                //若为白键，使用1指
                else {
                    note.finger = 1;
                }
                /*//遍历5个指法，取第一个可用的指法
                for (var finger = 1; finger <= 5; finger++) {
                    var tmpNote = new SingleNote(0, 0, note.note);
                    tmpNote.finger = finger;
                    List<int> toNext = calcHigherByLower(tmpNote, next);
                    if (toNext.Count > 0) {
                        note.finger = finger;
                        return;
                    }
                }
                //若五个指法均不可用，则说明此音是二连跳的第一个音，可用1指
                */
            }
            //音高低于当前音
            else if(next.note < note.note) {
                //判断当前音是否是黑键
                if(note.isBlackKey) {
                    //判断下一个音是否是白键且在纯四度以内
                    if(!next.isBlackKey &&
                        note.getNoteDistance(next) <= Interval.PERFECT4)
                        note.finger = 3;
                    else
                        note.finger = 4;
                }
                //若为白键
                else {
                    note.finger = 5;
                }
                /*//遍历5个指法，取第一个可用的指法
                for (var finger = 5; finger >= 1; finger--) {
                    var tmpNote = new SingleNote(0, 0, note.note);
                    tmpNote.finger = finger;
                    List<int> toNext = calcLowerByHigher(tmpNote, next);
                    if (toNext.Count > 0) {
                        note.finger = finger;
                        return;
                    }
                }
                note.finger = 5;*/
            }
            //等于当前音，用3指
            else {
                note.finger = 3;
            }
        }
        //后一个音为复音
        else {
            var next = _next as Chord;
            //当前音低于后一个复音的最低音
            if(note.note < next.minPitch) {
                //当前音是否是黑键，若是，则应当考虑是否用2指
                if(note.isBlackKey) {
                    //若后一个音的最高音与此音的音程大于纯四度，用1指
                    if(note.getNoteDistance(next.maxNote) > Interval.PERFECT4) {
                        note.finger = 1;
                    }
                    //等于纯四度且为黑键，用1指
                    else if(note.getNoteDistance(next.maxNote) == Interval.PERFECT4 &&
                        next.minNote.isBlackKey) {
                        note.finger = 1;
                    }
                    //小于纯四度
                    //通常情况下，用2指
                    else {
                        note.finger = 2;
                    }
                }
                //若为白键，使用1指
                else {
                    note.finger = 1;
                }
            }
            //高于复音的最高音
            else if(note.note > next.maxPitch) {
                //判断当前音是否是黑键
                if(note.isBlackKey) {
                    //判断复音的最低音是否是白键且在纯四度以内
                    if(!next.minNote.isBlackKey &&
                        note.getNoteDistance(next.minNote) <= Interval.PERFECT4)
                        note.finger = 3;
                    else
                        note.finger = 4;
                }
                //若为白键
                else {
                    note.finger = 5;
                }
            }
            //其他情况
            else {
                note.finger = 3;
            }
        }
    }

    //此方法对右手的逻辑进行复制和修改，如修改了右手逻辑，此逻辑应当全部重写
    private void calcLeftJumpToNote(SingleNote note) {
        note.type = Note.PlayType.JUMP;
        //需要跳跃到达的音通常应忽略前一个音
        //若当前音为音符列表最后一个音，则应考虑前一个音
        if(note.index == noteList.Count - 1) {
            Note _prev = noteList[note.index - 1];
            if(_prev is SingleNote) {
                var prev = _prev as SingleNote;
                //左手是跳到较高音用1指，较低音用5指
                note.finger = prev.note > note.note ? 5 : 1;
            } else if(_prev is Chord) {
                var prev = _prev as Chord;
                if(note.note > prev.maxPitch)
                    note.finger = 1;
                else if(note.note < prev.minPitch)
                    note.finger = 5;
                else note.finger = 3;
            }
            return;
        }
        //若不是，则仅考虑后一个音即可
        Note _next = noteList[note.index + 1];
        //后一个音为单音
        if(_next is SingleNote) {
            var next = _next as SingleNote;
            //音高【低】于当前音
            if(next.note < note.note) {
                //当前音是否是黑键，若是，则应当考虑是否用2指
                if(note.isBlackKey) {
                    //若后一个音与此音的音程大于纯四度，用1指
                    if(note.getNoteDistance(next) > Interval.PERFECT4) {
                        note.finger = 1;
                    }
                    //等于纯四度且为黑键，用1指
                    else if(note.getNoteDistance(next) == Interval.PERFECT4 &&
                        next.isBlackKey) {
                        note.finger = 1;
                    }
                    //小于纯四度
                    //通常情况下，用2指
                    else {
                        note.finger = 2;
                    }
                }
                //若为白键，使用1指
                else {
                    note.finger = 1;
                }
                /*//遍历5个指法，取第一个可用的指法
                for (var finger = 1; finger <= 5; finger++) {
                    var tmpNote = new SingleNote(0, 0, note.note);
                    tmpNote.finger = finger;
                    List<int> toNext = calcHigherByLower(tmpNote, next);
                    if (toNext.Count > 0) {
                        note.finger = finger;
                        return;
                    }
                }
                //若五个指法均不可用，则说明此音是二连跳的第一个音，可用1指
                */
            }
            //音高【高】于当前音
            else if(next.note > note.note) {
                //判断当前音是否是黑键
                if(note.isBlackKey) {
                    //判断下一个音是否是白键且在纯四度以内
                    if(!next.isBlackKey &&
                        note.getNoteDistance(next) <= Interval.PERFECT4)
                        note.finger = 3;
                    else
                        note.finger = 4;
                }
                //若为白键
                else {
                    note.finger = 5;
                }
                /*//遍历5个指法，取第一个可用的指法
                for (var finger = 5; finger >= 1; finger--) {
                    var tmpNote = new SingleNote(0, 0, note.note);
                    tmpNote.finger = finger;
                    List<int> toNext = calcLowerByHigher(tmpNote, next);
                    if (toNext.Count > 0) {
                        note.finger = finger;
                        return;
                    }
                }
                note.finger = 5;*/
            }
            //等于当前音，用3指
            else {
                note.finger = 3;
            }
        }
        //后一个音为复音
        else {
            var next = _next as Chord;
            //当前音低于后一个复音的最低音
            if(note.note < next.minPitch) {
                //判断当前音是否是黑键
                if(note.isBlackKey) {
                    //判断最高音是否是白键且在纯四度以内
                    if(!next.maxNote.isBlackKey &&
                        note.getNoteDistance(next.maxNote) <= Interval.PERFECT4)
                        note.finger = 3;
                    else
                        note.finger = 4;
                }
                //若为白键
                else {
                    note.finger = 5;
                }
            }
            //高于复音的最高音
            else if(note.note > next.maxPitch) {
                //当前音是否是黑键，若是，则应当考虑是否用2指
                if(note.isBlackKey) {
                    //若后一个音的最低音与此音的音程大于纯四度，用1指
                    if(note.getNoteDistance(next.minNote) > Interval.PERFECT4) {
                        note.finger = 1;
                    }
                    //等于纯四度且为黑键，用1指
                    else if(note.getNoteDistance(next.minNote) == Interval.PERFECT4 &&
                        next.minNote.isBlackKey) {
                        note.finger = 1;
                    }
                    //小于纯四度
                    //通常情况下，用2指
                    else {
                        note.finger = 2;
                    }
                }
                //若为白键，使用1指
                else {
                    note.finger = 1;
                }
            }
            //其他情况
            else {
                note.finger = 3;
            }
        }
    }

    /// <summary>
    /// 基于右手（实际上左手也用的是这套逻辑），计算当后一个音与前一个音均为单音，
    /// 且音高相同时，后一个音推荐的指法
    /// </summary>
    /// <param name="prev"></param>
    /// <returns></returns>
    private List<int> calcSameNote(SingleNote prev) {
        //对于右手而言，小于前一个音所用的指法的手指均最推荐使用
        //若前一个音使用1指，则推荐使用4、3、2指作为后一个音的指法
        //越靠近上一个音所用手指的越推荐
        switch(prev.finger) {
            case 1:
                return newList(4, 3, 1, 2);
            case 2:
                return newList(1, 2, 3, 4);
            case 3:
                return newList(2, 1, 3, 4);
            case 4:
                return newList(3, 4, 2, 1);
            case 5:
                return newList(4, 3, 2, 1);
            default:
                throw new Exception("提供的指法不正确");
        }
    }

    /// <summary>
    /// 基于右手，计算从较高单音到较低单音时，较低单音推荐使用的指法
    /// 需保证：前一个单音高于后一个单音
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    public List<int> calcLowerByHigher(SingleNote prev, SingleNote note) {
        if(prev.finger == -1) throw new Exception("前一个音尚未分配指法");
        //两音的音程
        int distance = note.getNoteDistance(prev);
        //若前一个音为黑键
        if(prev.isBlackKey) {
            //若当前音为黑键（黑-黑）
            if(note.isBlackKey) {
                //前一个音与当前音均为黑键
                switch(prev.finger) {
                    case 1:
                        if(distance <= Interval.BIG2)
                            return newList(2, 3, 4);
                        if(distance <= Interval.PERFECT4)
                            return newList(2, 3);
                        if(distance <= Interval.PERFECT5)
                            return newList(2);
                        return newList();
                    case 2:
                        if(distance <= Interval.BIG6)
                            return newList(1);
                        return newList();
                    case 3:
                        if(distance <= Interval.BIG2)
                            return newList(2, 1);
                        if(distance <= Interval.PERFECT4)
                            return newList(1, 2);
                        if(distance <= Interval.SMALL7)
                            return newList(1);
                        return newList();
                    case 4:
                        if(distance <= Interval.BIG2)
                            return newList(3, 2, 1);
                        if(distance <= Interval.SMALL3)
                            return newList(2, 3, 1);
                        if(distance <= Interval.BIG3)
                            return newList(2, 1, 3);
                        if(distance <= Interval.PERFECT4)
                            return newList(2, 1);
                        if(distance <= Interval.PERFECT5)
                            return newList(1, 2);
                        if(distance <= Interval.PERFECT8)
                            return newList(1);
                        return newList();
                    case 5:
                        if(distance <= Interval.BIG2)
                            return newList(4, 3, 2, 1);
                        if(distance <= Interval.BIG3)
                            return newList(3, 2, 4, 1);
                        if(distance <= Interval.PERFECT4)
                            return newList(2, 3, 1);
                        if(distance <= Interval.BIG6)
                            return newList(1, 2);
                        if(distance <= Interval.BIG9)
                            return newList(1);
                        return newList();
                }
            }
            //黑-白
            else {
                switch(prev.finger) {
                    case 1:
                        if(distance <= Interval.SMALL3)
                            return newList(2, 3);
                        if(distance <= Interval.BIG3)
                            return newList(2);
                        return newList();
                    case 2:
                        if(distance <= Interval.SMALL6)
                            return newList(1);
                        return newList();
                    case 3:
                        if(distance <= Interval.BIG2)
                            return newList(2, 1);
                        if(distance <= Interval.PERFECT4)
                            return newList(1, 2);
                        if(distance <= Interval.SMALL7)
                            return newList(1);
                        return newList();
                    case 4:
                        if(distance <= Interval.BIG2)
                            return newList(3, 2, 1);
                        if(distance <= Interval.SMALL3)
                            return newList(2, 3, 1);
                        if(distance <= Interval.BIG3)
                            return newList(2, 1, 3);
                        if(distance <= Interval.PERFECT4)
                            return newList(2, 1);
                        if(distance <= Interval.PERFECT5)
                            return newList(1, 2);
                        if(distance <= Interval.BIG7)
                            return newList(1);
                        return newList();
                    case 5:
                        if(distance <= Interval.BIG2)
                            return newList(4, 3, 1);
                        if(distance <= Interval.SMALL3)
                            return newList(3, 4, 2, 1);
                        if(distance <= Interval.BIG3)
                            return newList(2, 3, 1);
                        if(distance <= Interval.PERFECT4)
                            return newList(2, 1, 3);
                        if(distance <= Interval.PERFECT5)
                            return newList(2, 1);
                        if(distance <= Interval.BIG6)
                            return newList(1, 2);
                        if(distance <= Interval.SMALL9)
                            return newList(1);
                        return newList();
                }
            }
        }
        //前一个音为白键
        else {
            //白-黑
            if(note.isBlackKey) {
                switch(prev.finger) {
                    case 1:
                        if(distance <= Interval.SMALL2)
                            return newList(4, 3, 2);
                        if(distance <= Interval.BIG3)
                            return newList(3, 2, 4);
                        if(distance <= Interval.PERFECT4)
                            return newList(3, 2);
                        if(distance <= Interval.DIMIN5)
                            return newList(2);
                        return newList();
                    case 2:
                        if(distance <= Interval.SMALL6)
                            return newList(1);
                        return newList();
                    case 3:
                        if(distance <= Interval.SMALL3)
                            return newList(2, 1);
                        if(distance <= Interval.BIG3)
                            return newList(1, 2);
                        if(distance <= Interval.SMALL6)
                            return newList(1);
                        return newList();
                    case 4:
                        if(distance <= Interval.SMALL2)
                            return newList(3, 2);
                        if(distance <= Interval.SMALL3)
                            return newList(3, 2, 1);
                        if(distance <= Interval.DIMIN5)
                            return newList(2, 1);
                        if(distance <= Interval.BIG7)
                            return newList(1);
                        return newList();
                    case 5:
                        if(distance <= Interval.SMALL2)
                            return newList(4, 3, 2);
                        if(distance <= Interval.BIG2)
                            return newList(4, 3, 2, 1);
                        if(distance <= Interval.SMALL3)
                            return newList(3, 4, 2, 1);
                        if(distance <= Interval.PERFECT4)
                            return newList(3, 2, 1);
                        if(distance <= Interval.DIMIN5)
                            return newList(2, 3, 1);
                        if(distance <= Interval.SMALL6)
                            return newList(2, 1);
                        if(distance <= Interval.SMALL9)
                            return newList(1);
                        return newList();
                }
            }
            //白-白
            else {
                switch(prev.finger) {
                    case 1:
                        if(distance <= Interval.BIG3)
                            return newList(3, 2, 4);
                        if(distance <= Interval.PERFECT4)
                            return newList(2, 3);
                        return newList();
                    case 2:
                        if(distance <= Interval.BIG6)
                            return newList(1);
                        return newList();
                    case 3:
                        if(distance <= Interval.BIG2)
                            return newList(2, 1);
                        if(distance <= Interval.BIG3)
                            return newList(1, 2);
                        if(distance <= Interval.BIG7)
                            return newList(1);
                        return newList();
                    case 4:
                        if(distance <= Interval.BIG2)
                            return newList(3, 2, 1);
                        if(distance <= Interval.BIG3)
                            return newList(2, 1, 3);
                        if(distance <= Interval.PERFECT4)
                            return newList(2, 1);
                        if(distance <= Interval.PERFECT5)
                            return newList(1, 2);
                        if(distance <= Interval.PERFECT8)
                            return newList(1);
                        return newList();
                    case 5:
                        if(distance <= Interval.BIG2)
                            return newList(4, 3, 2, 1);
                        if(distance <= Interval.BIG3)
                            return newList(3, 4, 2, 1);
                        if(distance <= Interval.PERFECT4)
                            return newList(2, 1, 3);
                        if(distance <= Interval.BIG6)
                            return newList(1, 2);
                        if(distance <= Interval.BIG9)
                            return newList(1);
                        return newList();
                }
            }
        }
        //未能算得指法，建议跳跃
        return newList();
    }

    /// <summary>
    /// 为左手根据前一个较高单音计算后一个较低单音的指法
    /// 左手下行和右手上行可以使用一套逻辑，右手下行和左手上行可以使用一套逻辑
    /// 它们都没有比较绝对音高，而是比较音程
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    private List<int> calcLeftLowerByHigher(SingleNote prev, SingleNote note) {
        //前一个音高于后一个音
        return calcHigherByLower(prev, note);
    }

    /// <summary>
    /// 基于右手，计算从较低单音到较高单音时，较高单音推荐使用的指法
    /// 需保证：前一个单音低于后一个单音
    /// </summary>
    private List<int> calcHigherByLower(SingleNote prev, SingleNote note) {
        if(prev.finger == -1) throw new Exception("前一个音尚未分配指法");
        //若前一个音为5指，则此音一定通过跳跃到达
        if(prev.finger == 5) return newList();
        //两音的音程
        int distance = note.getNoteDistance(prev);
        //若前一个音为黑键
        if(prev.isBlackKey) {
            //若当前音为黑键（黑-黑）
            if(note.isBlackKey) {
                //前一个音与当前音均为黑键
                switch(prev.finger) {
                    case 1:
                        if(distance <= Interval.SMALL3)
                            return newList(2, 3, 4, 5);
                        //大三度
                        if(distance <= Interval.BIG3)
                            return newList(3, 2, 4, 5);
                        //纯五度
                        if(distance <= Interval.PERFECT5)
                            return newList(3, 4, 5, 2);
                        //大六度
                        if(distance <= Interval.BIG6)
                            //音符的顺序代表优先级
                            return newList(3, 4, 5, 2);
                        //纯八度
                        if(distance <= Interval.PERFECT8)
                            return newList(4, 5, 3);
                        //大九度
                        if(distance <= Interval.BIG9)
                            return newList(5);
                        //更大
                        //没有，表示建议跳跃到此音
                        return newList();
                    case 2:
                        //大二度
                        if(distance <= Interval.BIG2)
                            return newList(3, 4, 1, 5);
                        //大三度
                        if(distance <= Interval.BIG3)
                            return newList(4, 3, 1, 5);
                        //纯四度
                        if(distance <= Interval.PERFECT4)
                            return newList(4, 5, 3, 1);
                        //纯五度
                        if(distance <= Interval.PERFECT5)
                            return newList(5, 4);
                        //小七度
                        if(distance <= Interval.SMALL7)
                            return newList(5);
                        //其他
                        return newList();
                    case 3:
                        //小三度
                        if(distance <= Interval.SMALL3)
                            return newList(4, 5, 1);
                        //大三度
                        if(distance <= Interval.BIG3)
                            return newList(5, 1, 4);
                        //纯四度
                        if(distance <= Interval.PERFECT4)
                            return newList(5, 1);
                        //纯五度
                        if(distance <= Interval.PERFECT5)
                            return newList(5);
                        //其他
                        return newList();
                    case 4:
                        //小三度
                        if(distance <= Interval.SMALL3)
                            return newList(5, 1);
                        //大三度
                        if(distance <= Interval.BIG3)
                            return newList(5);
                        //其他
                        return newList();
                    //上行，不需要判断5指的情况
                }
            }
            //黑-白
            else {
                switch(prev.finger) {
                    case 1:
                        //大三度
                        if(distance <= Interval.BIG3)
                            return newList(2, 3, 4, 5);
                        //减五度
                        if(distance <= Interval.DIMIN5)
                            return newList(3, 2, 4, 5);
                        //小六度
                        if(distance <= Interval.SMALL6)
                            return newList(4, 3, 5, 2);
                        //大七度
                        if(distance <= Interval.BIG7)
                            return newList(4, 5, 3);
                        //大九度
                        if(distance <= Interval.BIG9)
                            return newList(5);
                        //其他
                        return newList();
                    case 2:
                        //小三度
                        if(distance <= Interval.SMALL3)
                            return newList(3, 1, 4, 5);
                        //大三度
                        if(distance <= Interval.BIG3)
                            return newList(4, 1, 5, 3);
                        //减五度
                        if(distance <= Interval.DIMIN5)
                            return newList(4, 5, 1);
                        //小七度
                        if(distance <= Interval.SMALL7)
                            return newList(5);
                        //其他
                        return newList();
                    case 3:
                        if(distance <= Interval.SMALL3)
                            return newList(4, 1, 5);
                        if(distance <= Interval.BIG3)
                            return newList(5, 1, 4);
                        if(distance <= Interval.DIMIN5)
                            return newList(5, 1);
                        return newList();
                    case 4:
                        if(distance <= Interval.BIG3)
                            return newList(5, 1);
                        return newList();
                }
            }
        }
        //前一个音为白键
        else {
            //白-黑
            if(note.isBlackKey) {
                switch(prev.finger) {
                    case 1:
                        if(distance <= Interval.SMALL3)
                            return newList(2, 3, 4, 5);
                        if(distance <= Interval.DIMIN5)
                            return newList(3, 2, 4, 5);
                        if(distance <= Interval.SMALL6)
                            return newList(3, 4, 2, 5);
                        if(distance <= Interval.SMALL7)
                            return newList(4, 3, 5);
                        if(distance <= Interval.SMALL9)
                            return newList(4, 5);
                        return newList();
                    case 2:
                        if(distance <= Interval.SMALL2)
                            return newList(3, 4);
                        if(distance <= Interval.SMALL3)
                            return newList(4, 3, 5);
                        if(distance <= Interval.DIMIN5)
                            return newList(4, 5);
                        if(distance <= Interval.SMALL7)
                            return newList(5);
                        return newList();
                    case 3:
                        if(distance <= Interval.SMALL2)
                            return newList(4);
                        if(distance <= Interval.SMALL3)
                            return newList(4, 5);
                        if(distance <= Interval.DIMIN5)
                            return newList(5);
                        return newList();
                    case 4:
                        if(distance <= Interval.SMALL3)
                            return newList(5);
                        return newList();
                }
            }
            //白-白
            else {
                switch(prev.finger) {
                    case 1:
                        if(distance <= Interval.PERFECT5)
                            return newList(2, 3, 4, 5);
                        if(distance <= Interval.BIG6)
                            return newList(3, 4, 5, 2);
                        if(distance <= Interval.BIG7)
                            return newList(4, 3, 5);
                        if(distance <= Interval.PERFECT8)
                            return newList(5, 4);
                        if(distance <= Interval.BIG9)
                            return newList(5);
                        return newList();
                    case 2:
                        if(distance <= Interval.BIG3)
                            return newList(4, 3, 1, 5);
                        if(distance <= Interval.DIMIN5)
                            return newList(4, 5, 1, 3);
                        if(distance <= Interval.PERFECT5)
                            return newList(5, 4);
                        if(distance <= Interval.BIG6)
                            return newList(5);
                        return newList();
                    case 3:
                        //不得把转指写在第一个，否则可能产生转指颤音
                        if(distance <= Interval.BIG2)
                            return newList(4, 1, 5);
                        if(distance <= Interval.BIG3)
                            return newList(5, 1, 4);
                        if(distance <= Interval.DIMIN5)
                            return newList(5, 1);
                        if(distance <= Interval.PERFECT5)
                            return newList(5);
                        return newList();
                    case 4:
                        if(distance <= Interval.BIG3)
                            return newList(5, 1);
                        return newList();
                }
            }
        }
        //未能算得指法，建议跳跃
        return newList();
    }

    private List<int> calcLeftHigherByLower(SingleNote prev, SingleNote note) {
        return calcLowerByHigher(prev, note);
    }

    /// <summary>
    /// 传入可用的指法，返回新的列表对象
    /// </summary>
    /// <param name="fingers"></param>
    /// <returns></returns>
    private List<int> newList(params int[] fingers) {
        return new List<int>(fingers);
    }
}

/// <summary>
/// 音程
/// </summary>
public static class Interval {

    public const int
        SMALL2 = 1,
        BIG2 = 2,
        SMALL3 = 3,
        BIG3 = 4,
        PERFECT4 = 5,
        DIMIN5 = 6,
        PERFECT5 = 7,
        SMALL6 = 8,
        BIG6 = 9,
        SMALL7 = 10,
        BIG7 = 11,
        PERFECT8 = 12,
        SMALL9 = 13,
        BIG9 = 14;
}

}