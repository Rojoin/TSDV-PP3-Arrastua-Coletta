using System.Collections.Generic;
using Item;
using Player;
using UnityEngine;
using UnityEngine.Events;

namespace Table
{
    internal enum CauldronState
    {
        Empty,
        Cooking,
        Done
    }

    public class Cauldron : Table
    {
        [SerializeField] private float timerMax;
        [SerializeField] private float currentTime = 0.0f;
        private CauldronState state;
        [SerializeField] private Potion defaultPotion;
        [SerializeField] private Transform potionTransform;
        [SerializeField] private Potion potionFromRecipe;
        [SerializeField] private KitchenObjectSO[] ingredientsInCauldron;
        [SerializeField] private PotionRecipeSO[] posiblePotions;
        [SerializeField] private int maxIngredientInCauldron = 3;
        private int currentIngredientCounter = 0;
        [SerializeField] private Animator spoon;
        private List<IngredientData> cauldronStuff = new List<IngredientData>();
        [Header("Events")]
        public UnityEvent<float> OnFillAmountUpdated = new UnityEvent<float>();
        public UnityEvent<Sprite> OnIngredientAdded = new UnityEvent<Sprite>();
        public UnityEvent OnCookingFinished = new UnityEvent();

  
      

        private void Start()
        {
            ResetCauldron();
        }

        private void Update()
        {
            if (state == CauldronState.Cooking)
            {
                if (currentTime > timerMax)
                {
                    if (currentIngredientCounter == maxIngredientInCauldron)
                    {
                        state = CauldronState.Done;
                        OnCookingFinished.Invoke();
                        PotionRecipeSO potionToGet = GetRecipePotion();
                        potionFromRecipe = InstantiatePotion(potionToGet);
                        cauldronStuff.Clear();
                        spoon.speed = 0;
                    }
                }
                else
                {
                    currentTime += Time.deltaTime;
                }

                float imageFillAmount = currentTime / timerMax;
                OnFillAmountUpdated.Invoke(imageFillAmount);
            }
        }

        public override void OnInteraction(PlayerInventory playerInventory = null, PlayerInteract playerInteract = null)
        {
            OnInteract.Invoke();
            switch (state)
            {
                case CauldronState.Done when !playerInventory.hasPickable():
                    playerInventory.SetPickable(potionFromRecipe);
                    ResetCauldron();
                    OnItemPickUp.Invoke();
                    break;
                case CauldronState.Empty when playerInventory.hasIngredient():
                case CauldronState.Cooking when playerInventory.hasIngredient():
                    if (!IsCauldronFull())
                    {
                        AddIngredientToCook(playerInventory.GetPickable() as Ingredient);
                        playerInventory.NullPickable();
                        OnItemDrop.Invoke();
                        spoon.speed = 1;
                    }
                    break;
                default:
                    OnFailedInteraction.Invoke();
                    break;
            }
        }

        private bool IsCauldronFull()
        {
            return currentIngredientCounter >= ingredientsInCauldron.Length;
        }

        private void AddIngredientToCook(Ingredient ingredient)
        {
            ingredientsInCauldron[currentIngredientCounter] = ingredient.GetSO();
            ingredient.SetNewParent(this.transform);


            OnIngredientAdded.Invoke(ingredient.GetIngredientImage());
            timerMax += ingredient.IsProcessed() ? ingredient.TimeToCook / 2.0f : ingredient.TimeToCook;
            state = CauldronState.Cooking;
            currentIngredientCounter++;

            KitchenObjectSO so = ingredient.GetSO();

            bool addNewIngredient = true;
            for (var i = 0; i < cauldronStuff.Count; i++)
            {
                var ingredientData = cauldronStuff[i];
                if (ingredientData.item == so)
                {
                    ingredientData.amount++;
                    addNewIngredient = false;
                }
            }

            if (addNewIngredient)
                cauldronStuff.Add(new IngredientData { item = so, amount = 1 });
            Destroy(ingredient.gameObject);
        }

        private void ResetCauldron()
        {
            ingredientsInCauldron = new KitchenObjectSO[maxIngredientInCauldron];
            currentTime = 0.0f;
            timerMax = 0.0f;
            state = CauldronState.Empty;
            spoon.speed = 0;
            currentIngredientCounter = 0;
        }
        
        private PotionRecipeSO GetRecipePotion()
        {
            PotionRecipeSO nextPotion = null;

            for (var i = 0; i < posiblePotions.Length; i++)
            {
                PotionRecipeSO recipe = posiblePotions[i];

                if (recipe.IsSameRecipe(cauldronStuff))
                    return recipe;
            }

            return null;
        }

        private Potion InstantiatePotion(PotionRecipeSO recipe)
        {
            Potion potion;
            if (recipe)
            {
                Debug.Log(recipe.name);
                potion = Instantiate(recipe.potion, potionTransform.position, Quaternion.identity);
            }
            else
                potion = Instantiate(defaultPotion, potionTransform.position, Quaternion.identity);

            potion.SetIconVisible(true);
            return potion;
        }
    }
}