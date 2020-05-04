using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HierarchicalGrouper
{
    private enum MultipleType
    {
        Invalid_Multiple,
        All_Transform,
        All_RectTransform,
    }

    private static Dictionary<UnityEngine.Object, int> _dontRepeat = new Dictionary<Object, int>();
    private static GameObject _group;

    [MenuItem("GameObject/Make Group", priority = 0, validate = false, menuItem = "GameObject/Make Group")]
    private static void MakeGroupMenu()
    {
        var res = CheckMultipleSelection();
        if (res == MultipleType.Invalid_Multiple)
        {
            Debug.LogError("Cannot");
            return;
        }

        var selections = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
        
        if (_dontRepeat.ContainsKey(Selection.activeObject))
        {
            _dontRepeat[Selection.activeObject] -= 1;
            if (_dontRepeat[Selection.activeObject] > 0)
            {
                return;
            }
            else
            {
                _dontRepeat.Remove(Selection.activeObject);
                if (_group != null)
                {
                    Selection.activeObject = _group;
                }
                _group = null;
                return;
            }
        }
        else
        {
            _dontRepeat.Add(Selection.activeObject, selections.Length - 1);
        }

        bool sameParents = true;
        Transform parent = null;
        Bounds bd = default(Bounds);
        if (res == MultipleType.All_Transform)
        {
            for (int i = 0; i < selections.Length; i++)
            {
                var sel = selections[i];


                if (parent == null)
                {
                    parent = sel.parent;
                }
                else
                {
                    if (parent != sel.parent)
                    {
                        sameParents = false;
                    }
                }
                if (i == 0)
                {
                    bd = new Bounds(sel.position, Vector3.zero);
                }
                else
                {
                    bd.Encapsulate(sel.position);
                }
            }

            _group = new GameObject("New Group");
            if (sameParents && parent != null)
            {
                _group.transform.SetParent(parent);
            }
            _group.transform.position = bd.center;

            foreach (var go in Selection.gameObjects)
            {
                go.transform.SetParent(_group.transform);
            }

        }
        else
        {
            for (int i = 0; i < selections.Length; i++)
            {
                var sel = selections[i] as RectTransform;

                if (parent == null)
                {
                    parent = sel.parent;
                }
                else
                {
                    if (parent != sel.parent)
                    {
                        sameParents = false;
                    }
                }

                Vector2 pos = sel.localPosition;
                Vector2 pivotOffset = sel.pivot - (Vector2.one * 0.5f);
                pos = new Vector2 { x = pos.x - (sel.rect.width * pivotOffset.x), y = pos.y - (sel.rect.height * pivotOffset.y) };
                if (i == 0)
                {
                    bd = new Bounds(pos, sel.rect.size);
                }
                else
                {
                    bd.Encapsulate(new Bounds(pos, sel.rect.size));
                }
            }

            _group = new GameObject("New Group");
            var groupRect = _group.AddComponent<RectTransform>();
            if (sameParents && parent != null)
            {
                _group.transform.SetParent(parent);
            }
            groupRect.localPosition = bd.center;

            groupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bd.size.x);
            groupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bd.size.y);

            foreach (var go in Selection.gameObjects)
            {
                go.transform.SetParent(groupRect);
            }
        }
    }

    //[MenuItem("GameObject/Make Group", validate = true)]
    private static bool MakeGroupMenuValidate()
    {
        return CheckMultipleSelection() != MultipleType.Invalid_Multiple;
    }

    private static MultipleType CheckMultipleSelection()
    {
        var selections = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
        bool multiple = (selections.Length > 1);
        if (!multiple)
        {
            EditorUtility.DisplayDialog("그룹화 실패", "단일 오브젝트는 그룹화될 수 없습니다.", "확인");
            return MultipleType.Invalid_Multiple;
        }

        int rectCounts = 0;
        foreach (var sel in selections)
        {
            if (sel is RectTransform)
            {
                rectCounts++;
            }
        }

        if (rectCounts > 0)
        {
            if (rectCounts == selections.Length)
            {
                Canvas canvas = null;
                foreach(var r in selections)
                {
                    var c = r.GetComponentInParent<Canvas>();
                    if (canvas == null)
                    {
                        canvas = c;
                    }
                    else if (c != canvas)
                    {
                        EditorUtility.DisplayDialog("그룹화 실패", "서로 다른 캔버스를 가진 오브젝트들은 그룹화될 수 없습니다.", "확인");
                        return MultipleType.Invalid_Multiple;
                    }
                }
                return MultipleType.All_RectTransform;
            }
            else
            {
                EditorUtility.DisplayDialog("그룹화 실패", "RectTransform과 Transform은 그룹화될 수 없습니다.", "확인");
                return MultipleType.Invalid_Multiple;
            }
        }
        else
        {
            return MultipleType.All_Transform;
        }
    }
}
