位于Algorithm.cs中

```csharp
/// <summary>
/// 如果到达上行序列中的某个音需要跳跃到达，调用此方法计算这个音的指法与演奏类型
/// </summary>
/// <param name="note">序列中的音</param>
/// <param name="seq">序列</param>
/// <param name="indexInSeq">音在序列中的位置</param>
private void calcJumpToNote(SingleNote note, NoteSequence seq, int indexInSeq) {
    //获取序列指定位置的音符，若越界返回null
    SingleNote getNote(int index) {
        try {
            return seq.notes[index];
        } catch (Exception e) {
            return null;
        }
    }
    
    note.type = Note.PlayType.JUMP;
    //需要跳跃到达的音通常应忽略前一个音
    //若当前音为音符列表最后一个音，则应考虑前一个音
    if (note.index == noteList.Count - 1) {
        //当前音是音符列表中最后一个音，则也一定是序列中最后一个音，且它的前一个音也一定是序列中的音（序列最少含2个音），前一个音一定低于当前音，且当前音需要跳跃到达
        note.finger = 5;
        return;
        /*Note _prevNote = noteList[note.index - 1];
        switch (_prevNote) {
            case SingleNote prevNote:
                //前一个音的音高大于当前音，用1指，否则用5指
                note.finger = prevNote.note > note.note ? 1 : 5;
                break;
            case Chord prevChord:
                //序列中的某个音的前一个音是复音，则此音一定是序列中第一个音，同时又是音符列表中最后一个音，通常不可能出现
                //前一个音最高音小于当前音，用5指
                if (prevChord.maxPitch < note.note) note.finger = 5;
                //最低音高于当前音，用1指
                else if (prevChord.minPitch > note.note) note.finger = 1;
                //其他情况（前一个音是复音，当前音是单音，且需要跳跃到达当前音），通常不可能出现
                else note.finger = 1;
                break;
        }
        return;*/
    }
    //若不是，则仅考虑后一个音即可
    //若为序列中最后一个音，则其后一个音只可能是低于当前音的单音，或任意复音
    if (indexInSeq == seq.notes.Count - 1) {
        Note _nextNote = noteList[note.index + 1];
        switch (_nextNote) {
            case SingleNote nextNote:
                /*//距下一个音大于或等于八度
                if (note.getNoteDistance(nextNote) >= 12) {
                    note.finger = 5;
                } else {
                    //若为白键
                    if (!note.isBlackKey) note.finger = 5;
                    //若为黑键
                    else {
                        //下一个音也是黑键
                        if (nextNote.isBlackKey) note.finger = 5;
                        
                    }
                }*/
                //通常使用5指，仅在当前音是黑键，下一个音不是黑键时，用4指
                if (note.isBlackKey && !nextNote.isBlackKey) note.finger = 4;
                else note.finger = 5;
                break;
            case Chord nextChord:
                //当前音小于或等于复音的最低音，用1指
                if (note.note <= nextChord.minPitch) note.finger = 1;
                //大于或等于复音的最高音，用4指或5指
                else if (note.note >= nextChord.maxPitch) {
                    //若为白键
                    if (!note.isBlackKey) note.finger = 5;
                    //若为黑键
                    else {
                        //下一个复音的最低音也是黑键，且与当前音距离大于纯五度，用5指，否则用4指
                        note.finger =
                            MovingData.isBlackKey(nextChord.minPitch) &&
                            note.getNoteDistance(nextChord.minPitch) > 7 ? 5 : 4;
                    }
                }
                //介于复音的最高音与最低音之间
                else {
                    //判断离哪个音更近
                    int toLeft = note.getNoteDistance(nextChord.minPitch);
                    int toRight = note.getNoteDistance(nextChord.maxPitch);
                    //离左边更近
                    if (toLeft <= toRight) {
                        note.finger = 2;
                    }
                    //离右边更近
                    else {
                        //距离大于大二度用3指，否则用4指
                        note.finger = toRight > 2 ? 3 : 4;
                    }
                }
                break;
        }
        return;
    }
    //若不为序列中最后一个音，其后一个音必定是单音且高于当前音
    //若为白键
    if (!note.isBlackKey) note.finger = 1;
    //若为黑键
    else {
        SingleNote nextNote = getNote(indexInSeq + 1);
        //若后一个音与此音的音程大于纯四度，用1指
        if (note.getNoteDistance(nextNote) > 5) {
            note.finger = 1;
        }
        //等于纯四度且为黑键，用1指
        else if (note.getNoteDistance(nextNote) == 5 && nextNote.isBlackKey) {
            note.finger = 1;
        }
        //小于纯四度
        //判断是否是三连黑键，若是，此键安排1指
        //若访问第三个键时越界，则不符合
        else if (getNote(indexInSeq + 2) == null) {
            note.finger = 2;
        } else if (nextNote.isBlackKey && getNote(indexInSeq + 2).isBlackKey) {
            note.finger = 1;
        }
        //不是三连黑键
        //后一个音是黑键，再后一个音是白键，且再后一个音距离后一个音大于或等于大三度，用1指
        else if (nextNote.isBlackKey && !getNote(indexInSeq + 2).isBlackKey &&
            nextNote.getNoteDistance(getNote(indexInSeq + 2)) >= 4) {
            note.finger = 1;
        }
        //通常情况下，用2指
        //TODO 有一种情况必须用3指，(#G #A) (#G #F F)，应当如何处理？
        //答复：并不是必须用3指，可以2 4 3 2 1，只是如何判断缩指的问题
        else {
            note.finger = 2;
        }
    }
}
```