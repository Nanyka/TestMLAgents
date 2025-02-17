using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace JumpeeIsland
{
    public class CreatureBuyButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IConfirmFunction
    {
        [SerializeField] private GameObject m_Container;
        [SerializeField] private Image m_ItemIcon;
        [SerializeField] private TextMeshProUGUI m_Price;
        
        // private AsyncOperationHandle m_UCDObjectLoadingHandle;
        protected CreatureMenu m_CreatureMenu;
        protected JIInventoryItem m_CreatureItem;
        protected Vector3 _spawnPosition;
        private int _layerMask = 1 << 6;
        private Camera _camera;

        private void OnEnable()
        {
            _camera = Camera.main;
        }

        public virtual void TurnOn(JIInventoryItem creatureItem, CreatureMenu creatureMenu)
        {
            m_CreatureItem = creatureItem;
            m_ItemIcon.sprite = AddressableManager.Instance.GetAddressableSprite(m_CreatureItem.spriteAddress);
            if (m_CreatureMenu == null)
                m_CreatureMenu = creatureMenu;

            var costs = SavingSystemManager.Instance.GetPurchaseCost(creatureItem.virtualPurchaseId);
            if (costs.Count > 0)
                m_Price.text = costs[0].amount.ToString();
            
            m_Container.SetActive(true);
        }

        public void TurnOff()
        {
            m_Container.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_CreatureMenu.StartADeal(m_CreatureItem.skinAddress[0]);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var ray = _camera.ScreenPointToRay(eventData.position);
            if (!Physics.Raycast(ray, out var collidedTile, 100f, _layerMask))
                return;
            
            m_CreatureMenu.SelectLocation(collidedTile.transform.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var collidedTile, 100f, _layerMask))
                return;

            _spawnPosition = collidedTile.transform.position;
            m_CreatureMenu.EndSelectionPhase(this, m_CreatureItem);
        }

        public virtual void ClickYes()
        {
            SavingSystemManager.Instance.OnTrainACreature(m_CreatureItem,_spawnPosition, true);
        }

        public Entity GetEntity()
        {
            throw new System.NotImplementedException();
        }
    }
}