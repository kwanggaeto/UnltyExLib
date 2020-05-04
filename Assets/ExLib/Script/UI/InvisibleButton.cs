using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ExLib.UI
{
    public class InvisibleButton : InvisibleGraphic, 
        IPointerClickHandler, 
        IPointerDownHandler, IPointerUpHandler, 
        IPointerEnterHandler, IPointerExitHandler, 
        IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler,
        ISelectHandler, IDeselectHandler
    {
        public bool enabledEnter;
        public bool enabledExit;
        public bool enabledPress;
        public bool enabledRelease;
        public bool enabledClick;
        public bool enabledBeginDrag;
        public bool enabledDrag;
        public bool enabledEndDrag;
        public bool enabledInitializePotetialDrag;

        public GameObject passEnterReceiver;
        public UnityEngine.Events.UnityEvent onEnter;
        
        public GameObject passExitReceiver;
        public UnityEngine.Events.UnityEvent onExit;
        
        public GameObject passPressReceiver;
        public UnityEngine.Events.UnityEvent onPress;
        
        public GameObject passReleaseReceiver;
        public UnityEngine.Events.UnityEvent onRelease;
        
        public GameObject passClickReceiver;
        public UnityEngine.Events.UnityEvent onClick;
        
        public GameObject passBeginDragReceiver;
        public UnityEngine.Events.UnityEvent onBeginDrag;
        
        public GameObject passDragReceiver;
        public UnityEngine.Events.UnityEvent onDrag;
        
        public GameObject passEndDragReceiver;
        public UnityEngine.Events.UnityEvent onEndDrag;
        
        public GameObject passInitializePotentialDragReceiver;
        public UnityEngine.Events.UnityEvent onInitializePotentialDrag;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!enabledClick)
                return;

            EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            if (passClickReceiver != null)
                ExecuteEvents.Execute(passClickReceiver, eventData, ExecuteEvents.pointerClickHandler);
            if (onClick != null)
                onClick.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!enabledPress)
                return;

            EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            if (passPressReceiver != null)
                ExecuteEvents.Execute(passPressReceiver, eventData, ExecuteEvents.pointerDownHandler);
            if (onPress != null)
                onPress.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!enabledRelease)
                return;
            if (passReleaseReceiver != null)
                ExecuteEvents.Execute(passReleaseReceiver, eventData, ExecuteEvents.pointerUpHandler);
            if (onRelease != null)
                onRelease.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!enabledEnter)
                return;
            if (passEnterReceiver != null)
                ExecuteEvents.Execute(passEnterReceiver, eventData, ExecuteEvents.pointerEnterHandler);
            if (onEnter != null)
                onEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enabledExit)
                return;
            if (passExitReceiver != null)
                ExecuteEvents.Execute(passExitReceiver, eventData, ExecuteEvents.pointerExitHandler);
            if (onExit != null)
                onExit.Invoke();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!enabledBeginDrag)
                return;
            if (passBeginDragReceiver != null)
                ExecuteEvents.Execute(passBeginDragReceiver, eventData, ExecuteEvents.beginDragHandler);
            if (onBeginDrag != null)
                onBeginDrag.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!enabledDrag)
                return;
            if (passDragReceiver != null)
                ExecuteEvents.Execute(passDragReceiver, eventData, ExecuteEvents.dragHandler);
            if (onDrag != null)
                onDrag.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!enabledEndDrag)
                return;
            if (passEndDragReceiver != null)
                ExecuteEvents.Execute(passEndDragReceiver, eventData, ExecuteEvents.endDragHandler);
            if (onEndDrag != null)
                onEndDrag.Invoke();
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (!enabledInitializePotetialDrag)
                return;
            if (passInitializePotentialDragReceiver != null)
                ExecuteEvents.Execute(passInitializePotentialDragReceiver, eventData, ExecuteEvents.initializePotentialDrag);
            if (onInitializePotentialDrag != null)
                onInitializePotentialDrag.Invoke();
        }

        public void OnSelect(BaseEventData eventData)
        {
            //Debug.Log("Selected");
        }

        public void OnDeselect(BaseEventData eventData)
        {
            //Debug.Log("Deselected");
        }
    }
}
