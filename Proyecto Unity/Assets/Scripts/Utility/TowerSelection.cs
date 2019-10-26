using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LookAtCamera))]
public class TowerSelection : MonoBehaviour
{

    LookAtCamera lookCamera;
    Quaternion startRotation;
    Quaternion startRotationCanvas;
    Vector3 startCanvasPosition;

    public Vector3 canvasRotationAR;
    public Vector3 canvasPositionAR;

    [SerializeField]
    Transform canvas = default, rangeSprite = default;
    
    [SerializeField]
    Button upgradeButton = default;

    [SerializeField]
    TextMeshProUGUI upgradeText = default, sellText = default;

    public int upgradeCost = default;

    Tower tower = default;

    private void Awake()
    {
        lookCamera = GetComponent<LookAtCamera>();
        startRotation = transform.rotation;
        startRotationCanvas = canvas.transform.rotation;
        startCanvasPosition = canvas.transform.localPosition;
        gameObject.SetActive(false);
    }

    public void ARenabled(bool enabled)
    {
        if (enabled)
        {
            lookCamera.enabled = true;
            canvas.rotation = Quaternion.Euler(canvasRotationAR);
            canvas.localPosition = canvasPositionAR;
        }
        else
        {
            lookCamera.enabled = false;
            transform.rotation = startRotation;
            canvas.rotation = startRotationCanvas;
            canvas.localPosition = startCanvasPosition;
        }
    }

    public Tower Tower
    {
        get => tower;
        set
        {
            tower = value;

            if (tower != null)
            {
                Initialize();
                gameObject.SetActive(true);
            }
            else
            {
                tower = null;
                gameObject.SetActive(false);
            }

        }
    }

    void Initialize()
    {
        transform.position = tower.transform.position;
        rangeSprite.localScale = Vector3.one * tower.Range;
        sellText.text = tower.sellCost.ToString();
        if (tower.canUpgrade)
        {
            upgradeCost = tower.upgradeCost;
            upgradeButton.gameObject.SetActive(true);
            upgradeText.text = upgradeCost.ToString();
            UpdateCanBuy();
        }
        else
        {
            upgradeButton.gameObject.SetActive(false);
        }
    }

    public void UpdateCanBuy()
    {
        if (tower != null && upgradeButton.gameObject.activeSelf)
        {
            if (Game.EnoughCredits(upgradeCost))
            {
                upgradeButton.interactable = true;
            }
            else
            {
                upgradeButton.interactable = false;
            }
        }
    }

    public void Upgrade()
    {
        //Debug.Log("Upgrading tower for " + upgradeCost + "#");
        Game.LoseCredits(upgradeCost);
        tower.Upgrade();
        Initialize();
    }

    public void Sell()
    {
        //Debug.Log("Selling tower for " + tower.sellCost + "#");
        Game.EarnCredits(tower.sellCost);
        tower.Sell();
        GameBoard.FindPathsStatic();
        gameObject.SetActive(false);
    }

}
