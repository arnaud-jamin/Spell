using Spell;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Spell.Ui
{
    public class UiMouseCatcher : UIBehaviour, IPointerDownHandler
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class ReferenceSettings
        {
            [AutoFind]
            public GameManager gameManager = null;
        }

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        ReferenceSettings m_references = null;

        //-----------------------------------------------------------------------------------------
        public void OnPointerDown(PointerEventData eventData)
        {
            m_references.gameManager.OnPointerDown(eventData);
        }
    }
}