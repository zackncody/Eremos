using UnityEngine;
using UnityEngine.Events;

public interface IToggleable
{
    void Show();
    void Hide();
}

public class ClickableObject : MonoBehaviour, IToggleable
{
    [SerializeField] private UnityEvent _onClicked;
    [SerializeField] private GameObject _gameObject;
    
    [SerializeField] private Collider2D hitCollider;
    [SerializeField] private bool ishide = false;

    private void Start()
    {
        if (ishide)
            Hide();
        else
            Show();
    }

    public void Show()
    {
        _gameObject.SetActive(true);

        if (hitCollider != null)
            hitCollider.enabled = true;
    }

    public void Hide()
    {
        _gameObject.SetActive(false);
        

        if (hitCollider != null)
            hitCollider.enabled = false;
    }

    public void OnClicked()
    {
        _onClicked?.Invoke();
        //Debug.Log("Clicked");

        if (_gameObject.activeSelf)
            Hide();
        else
            Show();
        
    }
}