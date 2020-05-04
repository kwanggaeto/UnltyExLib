using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace ExLib.Utils
{
    public delegate bool Filteration<T, U>(T item, int index, U target, T[] array);
    public delegate bool Filteration<T>(T item, int index, T[] array);

    public static class CollectionExtensions
    {
        public static void Random(this Array array)
        {
            Array.Sort(array, new RandomSort());
        }

        public static void Random<T>(this List<T> list)
        {
            list.Sort(new RandomSort<T>());
        }

        public struct RandomSort : IComparer
        {
            public int Compare(object x, object y)
            {
                return (int)UnityEngine.Random.Range(-1.99f, 1.99f);
            }
        }

        public struct RandomSort<T> : IComparer<T>
        {
            public int Compare(T x, T y)
            {
                return (int)UnityEngine.Random.Range(-1.2f, 1.2f);
            }
        }
    }

    /// <summary>
    /// 배열 유틸 클래스
    /// </summary>
    public static class ArrayUtil
    {
        /// <summary>
        /// 배열 리사이즈 타입. 제거, 추가
        /// </summary>
        private enum ResizeMode
        {
            REMOVE = -1,
            ADD = 1,
        }

        /// <summary>
        /// 배열 리사이징
        /// </summary>
        /// <param name="offset">1 or -1</param>
        /// <param name="index">target index</param>
        private static void ResizeArray<T>(ref T[] targetArray, ResizeMode resizeMode, int index = -1, int length = 1)
        {
            int offset = (int)resizeMode * length;

            if (targetArray.Length + offset <= 0)
            {
                Array.Clear(targetArray, 0, targetArray.Length);
                Array.Resize<T>(ref targetArray, 0);
                return;
            }
            else if (targetArray.Length < index+length)
            {
                if (resizeMode == ResizeMode.ADD)
                    Array.Resize<T>(ref targetArray, index + length);
            }


            if (index > -1)
            {
                T[] newArray = new T[targetArray.Length + offset];
                if (index > 0)
                {
                    //int min = Mathf.Min(targetArray.Length, newArray.Length);
                    int len = index > targetArray.Length ? targetArray.Length : index;
                    //Debug.LogFormat("Target Array Length:{0}, New Array Length:{1}, Copy Length:{2}", targetArray.Length, newArray.Length, index);                        
                    Array.Copy(targetArray, 0, newArray, 0, len);
                }

                int start = index + (resizeMode == ResizeMode.ADD ? 0 : length);
                int destStart = index + (resizeMode == ResizeMode.ADD ? length : 0);
                int endLenth = targetArray.Length - start;
                endLenth = endLenth <= 0 ? targetArray.Length : endLenth;

                if (start <= targetArray.Length + offset)
                {
                    Array.ConstrainedCopy(targetArray, start, newArray, destStart, endLenth);
                }

                Array.Clear(targetArray, 0, targetArray.Length);

                Array.Resize<T>(ref targetArray, targetArray.Length + offset);

                Array.ConstrainedCopy(newArray, 0, targetArray, 0, newArray.Length);

                Array.Clear(newArray, 0, newArray.Length);

                newArray = null;
            }
            else
            {
                Array.Resize<T>(ref targetArray, targetArray.Length + offset);
            }
        }

        /// <summary>
        /// 해당 위치에 객체 추가(내부 메소드)
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="insertObject">추가할 객체</param>
        /// <param name="index">위치</param>
        private static void INTERNAL_InsertToArray<T>(ref T[] targetArray, T insertObject, int index)
        {
            index = index < -1 ? -1 : index;
            ResizeArray<T>(ref targetArray, ResizeMode.ADD, index);
            int position = index == -1 ? targetArray.Length - 1 : index;
            targetArray[position] = insertObject;
        }

        /// <summary>
        /// 배열 앞에 객체 추가
        /// </summary>
        /// <typeparam name="T">배열 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="insertObject">추가할 객체</param>
        public static void Unshift<T>(ref T[] targetArray, T insertObject)
        {
            INTERNAL_InsertToArray(ref targetArray, insertObject, 0);
        }

        /// <summary>
        /// 배열 끝에 객체 추가
        /// </summary>
        /// <typeparam name="T">배열 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="insertObject">추가할 객체</param>
        public static void Push<T>(ref T[] targetArray, T insertObject)
        {
            INTERNAL_InsertToArray(ref targetArray, insertObject, -1);
        }

        /// <summary>
        /// 배열 끝에 배열 추가
        /// </summary>
        /// <typeparam name="T">배열 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="insertObject">추가할 배열</param>
        public static void PushRange<T>(ref T[] targetArray, T[] insertArray)
        {
            int len = targetArray.Length;
            ResizeArray<T>(ref targetArray, ResizeMode.ADD, -1, insertArray.Length);
            Array.ConstrainedCopy(insertArray, 0, targetArray, len, insertArray.Length);
        }

        /// <summary>
        /// 배열의 해당 인덱스에 객체 추가
        /// </summary>
        /// <typeparam name="T">배열 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="insertObject">추가할 객체</param>
        /// <param name="index">추가할 위치</param>
        public static void Insert<T>(ref T[] targetArray, T insertObject, int index)
        {
            INTERNAL_InsertToArray(ref targetArray, insertObject, index);
        }

        /// <summary>
        /// 배열의 해당 인덱스에 배열 추가
        /// </summary>
        /// <typeparam name="T">배열 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="insertObject">추가할 배열</param>
        public static void InsertRange<T>(ref T[] targetArray, T[] insertArray, int index)
        {
            int len = targetArray.Length;
            ResizeArray<T>(ref targetArray, ResizeMode.ADD, index, insertArray.Length);
            Array.ConstrainedCopy(insertArray, 0, targetArray, index, insertArray.Length);
        }

        /// <summary>
        /// 배열의 해당 객체가 있는 인덱스 삭제
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="target">삭제할 대상</param>
        /// <returns>삭제 성공, 실패(오류 또는 객체가 배열에 포함되어있지 않을 때) </returns>
        public static bool Remove<T>(ref T[] targetArray, T target)
        {
            int idx = FindIndex(targetArray, target);
            if (idx < 0)
                return false;

            try
            {
                RemoveAt(ref targetArray, idx);
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 배열의 해당 인덱스 삭제
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="index">삭제할 인덱스</param>
        /// <returns>삭제 할 인덱스에 위치한 객체</returns>
        public static T RemoveAt<T>(ref T[] targetArray, int index)
        {
            if (index < 0)
                throw new IndexOutOfRangeException("index parameter cannot smaller than 0(zero)");
            if (index > targetArray.Length-1)
                throw new IndexOutOfRangeException("index parameter cannot larger than the length of the target array");

            T target = targetArray[index];

            ResizeArray(ref targetArray, ResizeMode.REMOVE, index);

            return target;
        }

        /// <summary>
        /// 배열의 첫번째 인덱스 삭제
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <returns>첫번째에 위치한 객체</returns>
        public static T Shift<T>(ref T[] targetArray)
        {
            return RemoveAt(ref targetArray, 0);
        }

        /// <summary>
        /// 배열의 마지막 인덱스 삭제
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <returns>마지막에 위치한 객체</returns>
        public static T Pop<T>(ref T[] targetArray)
        {
            return RemoveAt(ref targetArray, targetArray.Length-1);
        }

        /// <summary>
        /// 배열에서 객체의 위치 찾기
        /// </summary>
        /// <typeparam name="T">배열 타입</typeparam>
        /// <param name="findArray">대상 배열</param>
        /// <param name="target">찾을 객체</param>
        /// <returns>위치 인덱스. 찾기 실패 시 -1 반환</returns>
        public static int FindIndex<T>(T[] findArray, T target)
        {
            int match = -1;
            int i = 0;
            while (match == -1)
            {
                if (i >= findArray.Length)
                    break;
                if (findArray[i].Equals(target))
                {
                    match = i;
                    break;
                }
                i++;
            }

            return match;
        }

        /// <summary>
        /// 조건식에 맞는 객체를 찾고 인덱스 반환
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <param name="findArray">대상 배열</param>
        /// <param name="filter">조건식</param>
        public static int Find<T>(T[] findArray, Filteration<T> filter)
        {
            for (int i=0, len=findArray.Length; i<len; i++)
            {
                if (filter(findArray[i], i, findArray))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 대상과 일치되는 객체를 모두 찾아 새 배열로 반환.
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <param name="findArray">대상 배열</param>
        /// <param name="target">찾을 대상</param>
        /// <returns>대상과 일치하는 객체를 취합한 배열</returns>
        public static T[] Find<T>(T[] findArray, T target)
        {
            T[] match = new T[0];
            int i = 0;
            while (i < findArray.Length)
            {
                if (findArray[i].Equals(target))
                {
                    Push<T>(ref match, findArray[i]);
                }
                i++;
            }

            return match;
        }

        /// <summary>
        /// 대상과 일치되는 객체를 모두 찾아 새 배열로 반환.
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <typeparam name="U">매치 값 타입</typeparam>
        /// <param name="findArray">대상 배열</param>
        /// <param name="targetValue">매치 값</param>
        /// <param name="filter">필터링 함수</param>
        /// <returns></returns>
        public static int FindIndex<T, U>(T[] findArray, U targetValue, Filteration<T, U> filter)
        {
            for (int i = 0, len = findArray.Length; i < len; i++)
            {
                if (filter(findArray[i], i, targetValue, findArray))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 조건에 맞는 객체들만 취합하여 배열을 새로 만듦.
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <typeparam name="U">조건식의 조건 객체 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="target">조건식의 조건 객체</param>
        /// <param name="filter">조건식</param>
        /// <returns>취합된 새 배열</returns>
        public static T[] Filter<T, U>(T[] targetArray, U target, Filteration<T, U> filter)
        {
            T[] newArray = null;
            for (int i=0, len=targetArray.Length; i<len; i++)
            {
                if (filter(targetArray[i], i, target, targetArray))
                {
                    if (newArray == null)
                        newArray = new T[] { targetArray[i] };
                    else
                        Push(ref newArray, targetArray[i]);
                }
            }

            return newArray;
        }

        /// <summary>
        /// 조건에 맞는 객체들만 취합하여 배열을 새로 만듦.
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <typeparam name="U">조건식의 조건 객체 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="target">조건식의 조건 객체</param>
        /// <param name="filter">조건식</param>
        /// <returns>취합된 새 배열</returns>
        public static T[] Filter<T>(T[] targetArray, Filteration<T> filter)
        {
            T[] newArray = null;
            for (int i = 0, len = targetArray.Length; i < len; i++)
            {
                if (filter(targetArray[i], i, targetArray))
                {
                    if (newArray == null)
                        newArray = new T[] { targetArray[i] };
                    else
                        Push(ref newArray, targetArray[i]);
                }
            }

            return newArray;
        }

        /// <summary>
        /// 조건에 맞는 객체들만 취합하여 배열을 수정함.
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <typeparam name="U">조건식의 조건 객체 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="target">조건식의 조건 객체</param>
        /// <param name="filter">조건식</param>
        public static void Crop<T, U>(ref T[] targetArray, U target, Filteration<T, U> filter)
        {
            T[] newArray = null;
            for (int i = 0, len = targetArray.Length; i < len; i++)
            {
                if (filter(targetArray[i], i, target, targetArray))
                {
                    if (newArray == null)
                        newArray = new T[] { targetArray[i] };
                    else
                        Push(ref newArray, targetArray[i]);
                }
            }

            if (newArray == null)
                return;

            Array.Clear(targetArray, 0, targetArray.Length);
            Array.Resize(ref targetArray, targetArray.Length);
            Array.Copy(newArray, targetArray, newArray.Length);
        }

        /// <summary>
        /// 조건에 맞는 객체들만 취합하여 배열을 수정함.
        /// </summary>
        /// <typeparam name="T">배열 객체 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="target">조건식의 조건 객체</param>
        /// <param name="filter">조건식</param>
        public static void Crop<T>(ref T[] targetArray, Filteration<T> filter)
        {
            T[] newArray = null;
            for (int i = 0, len = targetArray.Length; i < len; i++)
            {
                if (filter(targetArray[i], i, targetArray))
                {
                    if (newArray == null)
                        newArray = new T[] { targetArray[i] };
                    else
                        Push(ref newArray, targetArray[i]);
                }
            }

            if (newArray == null)
                return;

            Array.Clear(targetArray, 0, targetArray.Length);
            Array.Resize(ref targetArray, newArray.Length);
            Array.Copy(newArray, targetArray, newArray.Length);
        }

        /// <summary>
        /// 배열에서 해당 조건식에 맞는 객체 찾기(배열 상의 매치되는 첫번째 인덱스 반환)
        /// </summary>
        /// <typeparam name="T">배열 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="filter">조건식</param>
        /// <returns>인덱스</returns>
        public static int IndexOf<T, U>(T[] targetArray, U target, Filteration<T, U> filter)
        {
            for (int i = 0, len = targetArray.Length; i < len; i++)
            {
                if (filter(targetArray[i], i, target, targetArray))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 배열에서 해당 조건식에 맞는 객체 찾기(배열 상의 매치되는 마지막 인덱스 반환)
        /// </summary>
        /// <typeparam name="T">배열 타입</typeparam>
        /// <param name="targetArray">대상 배열</param>
        /// <param name="filter">조건식</param>
        /// <returns>인덱스</returns>
        public static int LastIndexOf<T, U>(T[] targetArray, U target, Filteration<T, U> filter)
        {
            for (int i = targetArray.Length-1, len = 0; i >= len; i--)
            {
                if (filter(targetArray[i], i, target, targetArray))
                {
                    return i;
                }
            }

            return -1;
        }

        public static void Sort<T>(T[] targetArray, Comparison<T> comparer)
        {
            Array.Sort(targetArray, comparer);
        }

        public static void Clear<T>(ref T[] targetArray)
        {
            Array.Clear(targetArray, 0, targetArray.Length);
            Array.Resize(ref targetArray, 0);
        }

        public static string PrintArray<T>(T[] array)
        {
            string str = string.Empty;
            for (int i = 0; i < array.Length; i++)
            {
                str += array[i].ToString();
                if (i < array.Length - 1)
                    str += ", ";
            }

            return str;
        }
    }

    public static class ArrayExtenstions
    {
        /*public static void Empty<T>(this T[] array)
        {
            if (array != null)
                Clear(array);

            array = new T[0];
        }

        public static void Push<T>(this T[] array, T item)
        {
            if (array == null)
                Empty(array);
            
            ArrayUtil.Push(ref array, item);
        }

        public static T Pop<T>(this T[] array)
        {
            return ArrayUtil.Pop(ref array);
        }

        public static T Shift<T>(this T[] array)
        {
            return ArrayUtil.Shift(ref array);
        }

        public static void Unshift<T>(this T[] array, T item)
        {
            if (array == null)
                Empty(array);

            ArrayUtil.Unshift(ref array, item);
        }

        public static bool Remove<T>(this T[] array, T item)
        {
            if (array == null)
                return false;

            return ArrayUtil.Remove(ref array, item);
        }

        public static T RemoveAt<T>(this T[] array, int index)
        {
            return ArrayUtil.RemoveAt(ref array, index);
        }

        public static T[] Find<T>(this T[] array, T item)
        {
            return ArrayUtil.Find(array, item);
        }

        public static int FindIndex<T, U>(this T[] array, U targetValue, Filteration<T, U> filter)
        {
            return ArrayUtil.FindIndex(array, targetValue, filter);
        }

        public static int FindIndex<T>(this T[] array, T target)
        {
            return ArrayUtil.FindIndex(array, target);
        }

        public static T[] Filter<T, U>(this T[] array, U targetValue, Filteration<T, U> filter)
        {
            return ArrayUtil.Filter(array, targetValue, filter);
        }

        public static void Crop<T, U>(this T[] array, U targetValue, Filteration<T, U> filter)
        {
            ArrayUtil.Crop(ref array, targetValue, filter);
        }

        public static void Clear<T>(this T[] array)
        {
            ArrayUtil.Clear(ref array);
        }*/
    }
}
