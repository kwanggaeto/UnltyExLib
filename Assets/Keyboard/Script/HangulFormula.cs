using UnityEngine;
using System;
using System.Text;
using System.Collections;


namespace ExLib.Control.UIKeyboard
{
    public class HangulFormula : LanguageFormulaBase
    {
        private const string _beginTable = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ"; // 초성 문자셋
        private const string _middleTable = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ"; // 중성 문자셋
        private const string _endTable = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ"; // 종성 문자셋
        private const string _beginSplitable = "ㄲㄸㅃㅆㅉ"; // 쌍초성 문자셋
        private const string _middleSplitable = "ㅐㅒㅔㅖㅘㅙㅚㅝㅞㅟㅢ"; // 조합중성 문자셋
        private const string _endSplitable = "ㄲㄳㄵㄶㄺㄻㄼㄽㄾㄿㅀㅄㅆ"; // 쌍종성 문자셋
        private const string _endSplitable2 = "ㄱㅅㅈㅎㄱㅁㅂㅅㅌㅍㅎㅅㅅ"; // 쌍종성의 뒷글자 문자셋
        private const ushort _hangulBase = 0xAC00; // 한글 시작 유니코드 값
        private const ushort _hangulLast = 0xD79F; // 한글 끝 유니코드 값

        private bool _middleCombined; // 조합된 중성 여부 플래그
        private bool _endCombined; // 조합된 종성 여부 플래그
        private bool _combined; // 문자 조합 여부 플래시

        public HangulFormula() { }

        /// <summary>
        /// 초성인가?
        /// </summary>
        /// <param name="value">확인 할 글자</param>
        /// <returns>True/False</returns>
        public static bool IsBegin(char value)
        {
            return (_beginTable.IndexOf(value) > -1);
        }

        /// <summary>
        /// 중성인가?
        /// </summary>
        /// <param name="value">확인 할 글자</param>
        /// <returns>True/False</returns>
        public static bool IsMiddle(char value)
        {
            return (_middleTable.IndexOf(value) > -1);
        }

        /// <summary>
        /// 종성인가?
        /// </summary>
        /// <param name="value">확인 할 글자</param>
        /// <returns>True/False</returns>
        public static bool IsEnd(char value)
        {
            return (_endTable.IndexOf(value) > -1);
        }

        /// <summary>
        /// 쌍자음 초성이 될 수 있는가?
        /// </summary>
        /// <param name="a">초성 A</param>
        /// <param name="b">초성 B</param>
        /// <returns>쌍자음의 인덱스 번호(private "_beginTable" array). 쌍자음 초성으로 변환 불가 시 -1 반환</returns>
        private int IsAbleToDoubleBegin(char a, char b)
        {
            if (a == 'ㄱ' && b == 'ㄱ') return _beginTable.IndexOf('ㄲ');
            if (a == 'ㄷ' && b == 'ㄷ') return _beginTable.IndexOf('ㄸ');
            if (a == 'ㅂ' && b == 'ㅂ') return _beginTable.IndexOf('ㅃ');
            if (a == 'ㅅ' && b == 'ㅅ') return _beginTable.IndexOf('ㅆ');
            if (a == 'ㅈ' && b == 'ㅈ') return _beginTable.IndexOf('ㅉ');

            return -1;
        }

        /// <summary>
        /// 조합모음 중성이 될 수 있는가?
        /// </summary>
        /// <param name="a">중성 A</param>
        /// <param name="b">중성 B</param>
        /// <returns>조합모음의 인덱스 번호(private "_middleTable" array). 조합모음 중성으로 변환 불가 시 -1 반환</returns>
        private int IsAbleToDoubleMiddle(char a, char b)
        {
            if (a == 'ㅏ' && b == 'ㅣ') return _middleTable.IndexOf('ㅐ');
            if (a == 'ㅑ' && b == 'ㅣ') return _middleTable.IndexOf('ㅒ');
            if (a == 'ㅓ' && b == 'ㅣ') return _middleTable.IndexOf('ㅔ');
            if (a == 'ㅕ' && b == 'ㅣ') return _middleTable.IndexOf('ㅖ');
            if (a == 'ㅗ' && b == 'ㅏ') return _middleTable.IndexOf('ㅘ');
            if (a == 'ㅗ' && b == 'ㅐ') return _middleTable.IndexOf('ㅙ');
            if (a == 'ㅗ' && b == 'ㅣ') return _middleTable.IndexOf('ㅚ');
            if (a == 'ㅜ' && b == 'ㅓ') return _middleTable.IndexOf('ㅝ');
            if (a == 'ㅜ' && b == 'ㅔ') return _middleTable.IndexOf('ㅞ');
            if (a == 'ㅜ' && b == 'ㅣ') return _middleTable.IndexOf('ㅟ');
            if (a == 'ㅡ' && b == 'ㅣ') return _middleTable.IndexOf('ㅢ');

            return -1;
        }

        /// <summary>
        /// 쌍자음 종성이 될 수 있는가?
        /// </summary>
        /// <param name="a">종성 A</param>
        /// <param name="b">종성 B</param>
        /// <returns>쌍자음의 인덱스 번호(private "_endTable" array). 쌍자음 종성으로 변환 불가 시 -1 반환</returns>
        private int IsAbleToDoubleEnd(char a, char b)
        {
            if (a == 'ㄱ' && b == 'ㄱ') return _endTable.IndexOf('ㄲ');
            if (a == 'ㄱ' && b == 'ㅅ') return _endTable.IndexOf('ㄳ');
            if (a == 'ㄴ' && b == 'ㅈ') return _endTable.IndexOf('ㄵ');
            if (a == 'ㄴ' && b == 'ㅎ') return _endTable.IndexOf('ㄶ');
            if (a == 'ㄹ' && b == 'ㄱ') return _endTable.IndexOf('ㄺ');
            if (a == 'ㄹ' && b == 'ㅁ') return _endTable.IndexOf('ㄻ');
            if (a == 'ㄹ' && b == 'ㅂ') return _endTable.IndexOf('ㄼ');
            if (a == 'ㄹ' && b == 'ㅅ') return _endTable.IndexOf('ㄽ');
            if (a == 'ㄹ' && b == 'ㅌ') return _endTable.IndexOf('ㄾ');
            if (a == 'ㄹ' && b == 'ㅍ') return _endTable.IndexOf('ㄿ');
            if (a == 'ㄹ' && b == 'ㅎ') return _endTable.IndexOf('ㅀ');
            if (a == 'ㅂ' && b == 'ㅅ') return _endTable.IndexOf('ㅄ');
            if (a == 'ㅅ' && b == 'ㅅ') return _endTable.IndexOf('ㅆ');

            return -1;
        }

        /// <summary>
        /// 문자 지우기
        /// </summary>
        /// <param name="a">기존 텍스트</param>
        public override int DeleteCharacter(ref string a, int startIndex, int endIndex)
        {
            int count = Mathf.Abs(endIndex - startIndex);
            int start = Mathf.Min(startIndex, endIndex);

            if (count == 0 && start == 0)
                return 0;

            if (Keyboard.verbose) Debug.Log("문자 지우기");
            char[] buffer;
            if (a.Length == 0) // 지울 문자가 없음
            {
                if (Keyboard.verbose) Debug.Log("지울 문자가 없음");
                return 0;
            }
            if (_combined || start < a.Length || count > 1) // 문자가 조합된 상태
            {
                _combined = true;
                if (Keyboard.verbose) Debug.Log("문자가 조합된 상태");
                a = a.Remove(count == 0 ? start - 1 : start, count == 0 ? 1 : count);// 한 글자 지우기

                return count > 1?start:start - 1;
            }
            else // 문자가 조합 안된 상태
            {
                if (Keyboard.verbose) Debug.Log("문자가 조합 안된 상태");
                Split(a[a.Length - 1], out buffer); // 글자 분리

                a = a.Remove(a.Length - 1); // 분리되는 글자 지움
                if (buffer != null) // 분리 됨
                {
                    if (Keyboard.verbose) Debug.Log("분리 됨");
                    if (buffer[2] != '\0' && buffer[2] != ' ') // 받침이 있는 글자
                    {
                        if (Keyboard.verbose) Debug.Log("받침이 있는 글자");
                        if (_endCombined) // 조합된 받침임
                        {
                            _endCombined = false;
                            char[] buffer2;
                            SplitDoubleChild(buffer[2], out buffer2); // 쌍받침 분리 시도
                            if (buffer2 != null) // 쌍받침 분리됨
                            {
                                if (Keyboard.verbose) Debug.Log("쌍받침 분리됨");
                                a += Combine(buffer[0], buffer[1], buffer2[0]);
                            }
                            else // 쌍받침 분리 안됨
                            {
                                if (Keyboard.verbose) Debug.Log("쌍받침 분리 안됨");
                                a += Combine(buffer[0], buffer[1], ' ');
                            }
                        }
                        else // 조합된 받침이 아님
                        {
                            if (Keyboard.verbose) Debug.Log("쌍받침 분리 안됨");
                            a += Combine(buffer[0], buffer[1], ' ');
                        }
                        _combined = false; // 조합 안된 상태로
                        return start;
                    }
                    else if (buffer[1] != '\0' && buffer[1] != ' ') // 받침 없이 중성 모음이 있는 글자
                    {
                        if (Keyboard.verbose) Debug.Log("받침 없이 중성 모음이 있는 글자");
                        if (_middleCombined)
                        {
                            _middleCombined = false;
                            char[] buffer2;
                            SplitDoubleMother(buffer[1], out buffer2); // 쌍모음 분리 시도
                            if (buffer2 != null) // 쌍모음 분리됨
                            {
                                if (Keyboard.verbose) Debug.Log("쌍모음 분리됨");
                                a += Combine(buffer[0], buffer2[0], ' ');
                            }
                            else // 쌍모음 분리 안됨
                            {
                                if (Keyboard.verbose) Debug.Log("쌍모음 분리 안됨");
                                a += buffer[0];
                            }
                        }
                        else
                        {
                            a += buffer[0]; // 모음 지우고 결합
                        }
                        _combined = false; // 조합 안된 상태로
                        return start;
                    }
                    else
                    {
                        if (Keyboard.verbose) Debug.Log("초성만");
                        _combined = true; // 조합된 상태로 
                    }
                }
                else
                {
                    _combined = true; // 조합된 상태로 
                }
            }

            return start - 1;
        }

        /// <summary>
        /// 한글 문자 추가 메소드.
        /// </summary>
        /// <param name="a">기존 텍스트</param>
        /// <param name="b">새로 추가할 문자</param>
        public override int MakeCharacter(ref string a, char b, int startIndex, int endIndex)
        {
            int start = Mathf.Min(startIndex, endIndex);
            int count = Mathf.Abs(endIndex - startIndex);
            if (Keyboard.verbose) Debug.Log("한글 문자 추가 메소드");

            char[] buffer;

            if (a.Length == 0) // 기존 텍스트가 없을 시..
            {
                if (Keyboard.verbose) Debug.Log("기존 텍스트가 없음");
                a = b.ToString();
                return start + 1;
            }

            int returnIndex = start + 1;

            int end = start - 1 >= a.Length ? a.Length - 1 : start - 1;

            if (count>0)
            {
                buffer = null;

                a = a.Remove(start, count);
            }
            else
            {
                // 기존 텍스트의 마지막 문자 분해
                if (end < 0 || a.Length <= end)
                {
                    buffer = null;
                }
                else
                {
                    Split(a[end], out buffer);
                }
            }

            if (buffer == null) // 기존 마지막 문자 분리 안됨
            {
                int newEnd = end;
                if (Keyboard.verbose) Debug.Log("기존 마지막 문자 분리 안됨");
                _combined = false;
                _middleCombined = false;
                char temp = b;

                if (end >= 0 && a.Length > end)
                {
                    if (IsBegin(a[end]) && IsMiddle(b))
                    {
                        if (Keyboard.verbose) Debug.Log("초성");
                        if (Keyboard.verbose) Debug.LogFormat("{0}, {1}", a[end], b);
                        temp = Combine(a[end], b, ' ');
                        a = a.Remove(end, 1);
                        returnIndex = start;
                    }
                    else if (IsMiddle(a[end]) && IsMiddle(b))
                    {
                        if (Keyboard.verbose) Debug.Log("중성");
                        temp = MakeDoubleMiddle(a[end], b);
                        if (temp != '\0')
                        {
                            _middleCombined = true;
                            a = a.Remove(end, 1);
                            returnIndex = start;
                        }
                        else
                        {
                            _middleCombined = false;
                            temp = b;
                        }
                    }
                    else
                    {
                        temp = b;
                        newEnd = start;
                    }
                }
                else
                {
                    newEnd = a.Length;
                }

                //a = string.Concat(a, temp);
                a = a.Insert(newEnd, temp.ToString());
            }
            else if (buffer[1] == '\0') // 기존 마지막 문자가 자음이나 모음이 한 개
            {
                if (Keyboard.verbose) Debug.Log("기존 마지막 문자가 자음이나 모음이 한 개");
                _combined = false;
                char temp = MakeDoubleBegin(buffer[0], b); // 쌍자음 초성 만들기 시도
                if (temp != '\0') // 쌍자음 초성
                    a = temp.ToString();
                else
                    a = a.Insert(end, b.ToString());
            }
            else if (buffer[2] == '\0' || buffer[2] == ' ') // 기존 텍스트 마지막 문자의 받침이 없음
            {
                int newEnd = end;
                if (Keyboard.verbose) Debug.Log("기존 텍스트 마지막 문자의 받침이 없음");
                _combined = false;
                Debug.Log(buffer[1]=='ㅗ');
                char temp = MakeDoubleMiddle(buffer[1], b); // 조합형 모음(ㅐㅔㅒㅖㅘㅙㅚㅝㅞㅟㅢ) 만들기 시도
                if (Keyboard.verbose) Debug.LogFormat("{0}, {1}, {2}", buffer[1], b, temp);
                if (temp == '\0') // 조합형 모음 만들기 실패
                {
                    if (Keyboard.verbose) Debug.Log("조합형 모음 말들기 실패");
                    if (IsMiddle(b))
                        _middleCombined = false;
                    if (IsEnd(b)) // 추가할 문자가 종성
                    {
                        if (Keyboard.verbose) Debug.Log("추가할 문자가 종성");
                        char c = Combine(buffer[0], buffer[1], b);
                        if (IsBlankCharacter(c))
                        {
                            if (Keyboard.verbose) Debug.Log("없는 문자라서 분리 문자 삽입");
                            //a = string.Concat(a, b); // 없는 문자라서 분리 문자 삽입
                            a = a.Insert(newEnd+1, b.ToString());
                            return newEnd+2;
                        }
                        _endCombined = false;
                        a = a.Remove(newEnd, 1); // 기존 마지막 문자 지우기
                        //a = string.Concat(a, c); // 조합된 문자 삽입
                        a = a.Insert(newEnd, c.ToString());

                        returnIndex = start;
                    }
                    else
                    {
                        if (Keyboard.verbose) Debug.Log("추가할 문자가 종성이 아님");
                        newEnd = start;
                        //a = string.Concat(a, b); // 기존 문자에 입력 문자 추가
                        a = a.Insert(newEnd, b.ToString());

                        returnIndex = start + 1;
                    }
                }
                else // 조합형 모음(ㅐㅔㅒㅖㅘㅙㅚㅝㅞㅟㅢ)임
                {
                    if (Keyboard.verbose) Debug.Log("조합형 모음임");
                    _middleCombined = true;
                    temp = Combine(buffer[0], temp, ' '); // 문자 조합
                    a = a.Remove(end, 1);
                    //a = string.Concat(a, temp); // 기존 텍스트에 문자 추가
                    a = a.Insert(newEnd, temp.ToString());
                    returnIndex = start;
                }
            }
            else // 기존 텍스트 마지막 문자가 받침이 있음
            {
                int newEnd = end;
                char temp = b;
                if (IsMiddle(b)) // 입력된 문자가 모음
                {
                    if (Keyboard.verbose) Debug.Log("입력된 문자가 모음");
                    char[] buffer2;
                    SplitDoubleChild(buffer[2], out buffer2); // 기존 마지막 문자의 받침이 쌍받침인가?
                    if (buffer2 == null) // 기존 마지막 문자는 쌍받침이 아님
                    {
                        if (Keyboard.verbose) Debug.Log("기존 마지막 문자는 쌍받침이 아님");
                        _combined = false; // 조합완료 False
                        a = a.Remove(end, 1);
                        char newChar = Combine(buffer[0], buffer[1], ' ');
                        a = a.Insert(newEnd, newChar.ToString());
                        //a += Combine(buffer[0], buffer[1], ' ');
                        temp = Combine(buffer[2], b, ' ');
                        newEnd = end + 1;
                    }
                    else // 기존 마지막 문자는 쌍받침임
                    {
                        _combined = false; // 조합완료 False
                        if (Keyboard.verbose) Debug.Log("기존 마지막 문자는 쌍받침임");
                        a = a.Remove(end, 1);
                        if (_endCombined) // 입력된 문자가 기존의 받침과 쌍받침으로 조합됨
                        {
                            if (Keyboard.verbose) Debug.Log("입력된 문자가 기존의 받침과 쌍받침으로 조합됨");
                            char newChar = Combine(buffer[0], buffer[1], buffer2[0]);
                            a = a.Insert(newEnd, newChar.ToString());
                            //a += Combine(buffer[0], buffer[1], buffer2[0]); // 분해된 씽받침 중 앞 문자를 받침으로 기존 마지막 텍스트 조합
                            temp = Combine(buffer2[1], b, ' '); // 분해된 쌍받침의 뒷 문자를 초성으로 조합
                            newEnd = end + 1;
                        }
                        else // 입력된 문자가 쌍받침으로 입력됨
                        {
                            returnIndex = start;
                            if (Keyboard.verbose) Debug.Log("입력된 문자가 쌍받침으로 입력됨");
                            char newChar = Combine(buffer[0], buffer[1], ' ');
                            a = a.Insert(newEnd, newChar.ToString());
                            //a += Combine(buffer[0], buffer[1], ' '); // 기존 마지막 문자를 받침을 제외하고 다시 조합
                            temp = Combine(buffer[2], b, ' '); // 입력 문자와 기존 마지막 문자 받침을 초성으로 조합
                            newEnd = end + 1;
                        }
                    }
                }
                else if (IsEnd(b)) // 입력된 문자가 받침
                {
                    if (Keyboard.verbose) Debug.Log("입력된 문자가 받침");
                    temp = MakeDoubleEnd(buffer[2], b); // 쌍받침으로 만들기 시도
                    if (temp == '\0') // 쌍받침으로 만들 수 없음
                    {
                        if (Keyboard.verbose) Debug.Log("쌍받침으로 만들 수 없음");
                        _combined = true; // 글자 조합 완료
                        _middleCombined = false;
                        _endCombined = false; // 쌍받침으로 조합되지 않음
                        temp = b;
                        newEnd = end + 1;
                    }
                    else // 쌍받침으로 조합됨
                    {
                        if (Keyboard.verbose) Debug.Log("상받침으로 조합됨");
                        _combined = false; // 글자 조합 False
                        _endCombined = true; // 쌍받침으로 조합됨.
                        
                        a = a.Remove(end, 1);

                        returnIndex = start;

                        temp = Combine(buffer[0], buffer[1], temp); // 조합된 쌍받침으로 문자 조합

                        if (IsBlankCharacter(temp))
                        {
                            if (Keyboard.verbose) Debug.Log("없는 문자라서 분리 문자 삽입");
                            _combined = true; // 글자 조합 완료
                            _middleCombined = false;
                            _endCombined = false; // 쌍받침으로 조합되지 않음
                            char newChar = Combine(buffer[0], buffer[1], buffer[0]);
                            a = a.Insert(newEnd, newChar.ToString());
                            //a += Combine(buffer[0], buffer[1], buffer[2]);
                            temp = b;
                        }
                    }
                }
                else
                {
                    newEnd = end + 1;
                }
                //a = string.Concat(a, temp);
                a = a.Insert(newEnd, temp.ToString());
            }

            return returnIndex;
        }

        /// <summary>
        /// 쌍자음 초성 만들기
        /// </summary>
        /// <param name="a">조합할 자음 A</param>
        /// <param name="b">조합할 자음 B</param>
        /// <returns>조합된 쌍자음 초성. 실패 시 '\0' 반환</returns>
        private char MakeDoubleBegin(char a, char b)
        {
            if (a == '\0' || b == '\0') // 조합할 두 문자 중 하나가 빈 문자
                return '\0';
            char temp = '\0';
            if (_beginSplitable.IndexOf(a) == -1 && _beginSplitable.IndexOf(b) == -1) // 조합할 두 문자가 모두 쌍자음이 아닐 때
            {
                int idx = IsAbleToDoubleBegin(a, b); // 쌍자음으로 만들기 시도

                if (idx > -1) // 쌍자음임
                    temp = _beginTable[idx];
            }

            return temp;
        }

        /// <summary>
        /// 쌍모음 중성 만들기
        /// </summary>
        /// <param name="a">조합할 모음 A</param>
        /// <param name="b">조합할 모음 B</param>
        /// <returns>조합된 쌍모음 중성. 실패 시 '\0' 반환</returns>
        private char MakeDoubleMiddle(char a, char b)
        {
            if (a == '\0' || b == '\0') // 조합할 두 문자 중 하나가 빈 문자
                return '\0';

            char temp = '\0';
            if (_middleSplitable.IndexOf(a) < 4 && _middleSplitable.IndexOf(b) < 4) // 조합할 두 문자가 모두 쌍모음이 아닐 때
            {
                int idx = IsAbleToDoubleMiddle(a, b); // 쌍모음 만들기 시도

                if (idx > -1) // 쌍모음임
                    temp = _middleTable[idx];
            }

            return temp;
        }

        /// <summary>
        /// 쌍받침 만들기
        /// </summary>
        /// <param name="a">조합할 받침 A</param>
        /// <param name="b">조합할 받침 B</param>
        /// <returns>조합된 쌍받침 종성. 실패 시 '\0' 반환</returns>
        private char MakeDoubleEnd(char a, char b)
        {
            if (a == '\0' || b == '\0') // 조합할 두 문자 중 하나가 빈 문자
                return '\0';

            char temp = '\0';
            if (_endSplitable.IndexOf(a) == -1 && _endSplitable.IndexOf(b) == -1) // 조합할 두 문자가 모두 쌍받침이 아닐 때
            {
                int idx = IsAbleToDoubleEnd(a, b); // 쌍받침 만들기 시도

                if (idx > -1) // 쌍받침임
                {
                    temp = _endTable[idx];
                }
            }

            return temp;
        }

        /// <summary>
        /// 초성, 중성, 종성 조합
        /// </summary>
        /// <param name="begin">초성</param>
        /// <param name="middle">중성</param>
        /// <param name="end">종성</param>
        /// <returns>조합된 문자</returns>
        public char Combine(char begin, char middle, char end)
        {
            int bIdx, mIdx, eIdx;
            bIdx = _beginTable.IndexOf(begin);
            mIdx = _middleTable.IndexOf(middle);
            eIdx = _endTable.IndexOf(end);

            int unicode = _hangulBase + (bIdx * 21 + mIdx) * 28 + eIdx;

            char buffer = Convert.ToChar(unicode);

            return buffer;
        }

        /// <summary>
        /// 문자 초성, 중성, 종성으로 분리하기
        /// </summary>
        /// <param name="word">분리할 문자</param>
        /// <param name="bubffer">분리된 문자들을 담을 버퍼</param>
        public void Split(char word, out char[] bubffer)
        {
            int bIdx, mIdx, eIdx;
            ushort uTempCode = 0x0000;

            uTempCode = Convert.ToUInt16(word);

            if (uTempCode < _hangulBase || uTempCode > _hangulLast) // 한글 범위를 벗어나면 null반환
            {
                bubffer = null;
                return;
            }

            bubffer = new char[3];

            int unicode = uTempCode - _hangulBase;

            bIdx = unicode / (21 * 28);
            unicode = unicode % (21 * 28);
            mIdx = unicode / 28;
            unicode = unicode % 28;
            eIdx = unicode;

            bubffer[0] = _beginTable[bIdx]; // 초성
            bubffer[1] = _middleTable[mIdx]; // 중성
            bubffer[2] = _endTable[eIdx]; // 종성
        }

        /// <summary>
        /// 쌍자음 분리
        /// </summary>
        /// <param name="word">분리할 자음</param>
        /// <param name="buffer">분리된 문자들을 담을 버퍼</param>
        private void SplitDoubleChild(char word, out char[] buffer)
        {
            buffer = null;
            int loc = -1;
            int idx = _beginSplitable.IndexOf(word); // 분리할 수 있는 쌍자음 초성인가?
            if (idx > -1) // 분리할 수 있는 쌍자음 초성
            {
                loc = 0; // 초성
                goto Split; // 분리 코드로 건너뜀
            }
            idx = _endSplitable.IndexOf(word); // 분리 할 수 있는 쌍자음 종성인가?
            if (idx == -1) // 분리할 수 없는 쌍자음 종성
                return; // 쌍자음 초성, 종성 모두 분리 안됨

            loc = 1; // 종성

            Split: // 이하 분리 코드
            buffer = new char[2];

            if (loc == 0) // 초성일 때
            {
                if (Keyboard.verbose) Debug.LogFormat("{0}, 인덱스:{1}", "초성일 때", idx);
                idx = _beginTable.IndexOf(word);
                buffer[0] = buffer[1] = _beginTable[idx - 1];
            }
            else // 종성일 때
            {
                if (idx < 2)
                {
                    buffer[0] = 'ㄱ';
                }
                else if (idx < 4)
                {
                    buffer[0] = 'ㄴ';
                }
                else if (idx < 12)
                {
                    buffer[0] = 'ㄹ';
                }
                buffer[1] = _endSplitable2[idx];
            }
        }

        /// <summary>
        /// 쌍모음 분리
        /// </summary>
        /// <param name="word">분리할 모음</param>
        /// <param name="buffer">분리된 문자들을 담을 버퍼</param>
        private void SplitDoubleMother(char word, out char[] buffer)
        {
            buffer = null;
            int idx = _middleSplitable.IndexOf(word); // 분리할 수 있는 쌍모음 중성인가?
            if (idx == -1) // 분리할 수 없는 쌍모음 중성
                return;

            buffer = new char[2];

            if (word == 'ㅐ')
            {
                buffer[0] = 'ㅏ';
                buffer[1] = 'ㅣ';
            }
            if (word == 'ㅒ')
            {
                buffer[0] = 'ㅑ';
                buffer[1] = 'ㅣ';
            }
            if (word == 'ㅔ')
            {
                buffer[0] = 'ㅓ';
                buffer[1] = 'ㅣ';
            }
            if (word == 'ㅖ')
            {
                buffer[0] = 'ㅕ';
                buffer[1] = 'ㅣ';
            }
            if (word == 'ㅘ')
            {
                buffer[0] = 'ㅗ';
                buffer[1] = 'ㅏ';
            }
            if (word == 'ㅙ')
            {
                buffer[0] = 'ㅗ';
                buffer[1] = 'ㅐ';
            }
            if (word == 'ㅚ')
            {
                buffer[0] = 'ㅗ';
                buffer[1] = 'ㅣ';
            }
            if (word == 'ㅝ')
            {
                buffer[0] = 'ㅜ';
                buffer[1] = 'ㅓ';
            }
            if (word == 'ㅞ')
            {
                buffer[0] = 'ㅜ';
                buffer[1] = 'ㅔ';
            }
            if (word == 'ㅟ')
            {
                buffer[0] = 'ㅜ';
                buffer[1] = 'ㅣ';
            }
            if (word == 'ㅢ')
            {
                buffer[0] = 'ㅡ';
                buffer[1] = 'ㅣ';
            }
        }

        public override void Reset()
        {
            _combined = false;
            _endCombined = false;
            _middleCombined = false;
        }
    }
}
