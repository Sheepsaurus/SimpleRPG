using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Controls;
using Engine.ViewModels;
using Engine.Models;

namespace SimpleRPG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Monster currentMonster;
        private GameSession gameSession;

        public MainWindow()
        {

            gameSession = new GameSession();

            DataContext = gameSession;
            InitializeComponent();



            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            gameSession.CurrentPlayer.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

            UpdateUserInfo();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(gameSession.CurrentPlayer.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(gameSession.CurrentPlayer.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(gameSession.CurrentPlayer.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(gameSession.CurrentPlayer.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            //Does the location have any required items
            if (!gameSession.CurrentPlayer.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Document.Blocks.Add(new Paragraph(new Run("You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine)));
                return;
            }

            // Update the CurrentPlayer's current location
            gameSession.CurrentPlayer.CurrentLocation = newLocation;

            // Show/hide available movement buttons
            ToggleButtonVisibility(newLocation);

            // Display current location name and description
            rtbLocation.Document.Blocks.Clear();
            rtbLocation.Document.Blocks.Add(new Paragraph(new Run(newLocation.Name + Environment.NewLine)));
            rtbLocation.Document.Blocks.Add(new Paragraph(new Run(newLocation.Description + Environment.NewLine)));

            // Completely heal the CurrentPlayer
            gameSession.CurrentPlayer.HitPoints = gameSession.CurrentPlayer.MaximumHitPoints;            

            // Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                bool conditionals = gameSession.CurrentPlayer.HasThisQuest(newLocation.QuestAvailableHere) &&
                                   !gameSession.CurrentPlayer.CompletedThisQuest(newLocation.QuestAvailableHere) &&
                                    gameSession.CurrentPlayer.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                // The CurrentPlayer has all items required to complete the quest
                if (conditionals)
                {
                    // Display message
                    CompleteQuest(newLocation);
                    ReceiveReward(newLocation);
                    gameSession.CurrentPlayer.MarkQuestCompleted(newLocation.QuestAvailableHere);
                }
                else
                {
                    // Display the messages
                    ReceiveNewQuest(newLocation);

                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Document.Blocks.Add(new Paragraph(new Run(qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine)));
                            continue;
                        }
                        rtbMessages.Document.Blocks.Add(new Paragraph(new Run(qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine)));

                    }
                    AddNewLine();

                    // Add the quest to the CurrentPlayer's quest list
                    gameSession.CurrentPlayer.Quests.Add(new CurrentPlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            // Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Document.Blocks.Add(new Paragraph(new Run("You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine)));

                // Make a new monster, using the values from the standard monster in the World.Monster list
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage,
                    standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.HitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    currentMonster.LootTable.Add(lootItem);
                }
                FightMonster(true);
            }
            currentMonster = null;
            FightMonster(false);
            UpdateNotQuest();
            UpdateQuestListInUI();
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            // Determine the amount of damage to do to the monster
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            // Apply the damage to the monster's HitPoints
            currentMonster.HitPoints -= damageToMonster;

            // Display message
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run("You hit the " + currentMonster.Name + " for " + damageToMonster.ToString() + " points." + Environment.NewLine)));

            // Check if the monster is dead
            if (currentMonster.HitPoints <= 0)
            {
                // Monster is dead
                AddNewLine();
                rtbMessages.Document.Blocks.Add(new Paragraph(new Run("You defeated the " + currentMonster.Name + Environment.NewLine)));

                // Give CurrentPlayer experience points for killing the monster
                gameSession.CurrentPlayer.ExperiencePoints += currentMonster.RewardExperiencePoints;
                rtbMessages.Document.Blocks.Add(new Paragraph(new Run("You receive " + currentMonster.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine)));

                // Give CurrentPlayer gold for killing the monster 
                gameSession.CurrentPlayer.Gold += currentMonster.RewardGold;
                rtbMessages.Document.Blocks.Add(new Paragraph(new Run("You receive " + currentMonster.RewardGold.ToString() + " gold" + Environment.NewLine)));

                // Get random loot items from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                // Add items to the lootedItems list, comparing a random number to the drop percentage
                foreach (LootItem lootItem in currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                // If no items were randomly selected, then add the default loot item(s).
                if (lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in currentMonster.LootTable)
                    {
                        if (lootItem.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    }
                }

                // Add the looted items to the CurrentPlayer's inventory
                foreach (InventoryItem inventoryItem in lootedItems)
                {
                    gameSession.CurrentPlayer.AddItemToInventory(inventoryItem.Details);

                    if (inventoryItem.Quantity == 1)
                    {
                        SendYouLootMessage(inventoryItem, inventoryItem.Details.Name);
                    }
                    SendYouLootMessage(inventoryItem, inventoryItem.Details.NamePlural);
                }

                // Refresh CurrentPlayer information and inventory controls
                UpdateUserInfo();
                UpdateNotQuest();


                // Add a blank line to the messages box, just for appearance.
                AddNewLine();

                // Move CurrentPlayer to current location (to heal CurrentPlayer and create a new monster to fight)
                MoveTo(gameSession.CurrentPlayer.CurrentLocation);
            }
            else
            {
                // Monster is still alive

                // Determine the amount of damage the monster does to the CurrentPlayer
                int damageToCurrentPlayer = RandomNumberGenerator.NumberBetween(0, currentMonster.MaximumDamage);

                // Display message
                rtbMessages.Document.Blocks.Add(new Paragraph(new Run("The " + currentMonster.Name + " did " + damageTogameSession.CurrentPlayer.ToString() + " points of damage." + Environment.NewLine)));

                // Subtract damage from CurrentPlayer
                gameSession.CurrentPlayer.HitPoints -= damageToCurrentPlayer;

                // Refresh CurrentPlayer data in UI
                hitpointsValueLabel.Content = gameSession.CurrentPlayer.HitPoints.ToString();

                if (gameSession.CurrentPlayer.HitPoints <= 0)
                {
                    // Display message
                    rtbMessages.Document.Blocks.Add(new Paragraph(new Run("The " + currentMonster.Name + " killed you." + Environment.NewLine)));

                    // Move CurrentPlayer to "Home"
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            // Add healing amount to the CurrentPlayer's current hit points
            gameSession.CurrentPlayer.HitPoints = (gameSession.CurrentPlayer.HitPoints + potion.AmountToHeal);

            // HitPoints cannot exceed CurrentPlayer's MaximumHitPoints
            if (gameSession.CurrentPlayer.HitPoints > gameSession.CurrentPlayer.MaximumHitPoints)
            {
                gameSession.CurrentPlayer.HitPoints = gameSession.CurrentPlayer.MaximumHitPoints;
            }

            // Remove the potion from the CurrentPlayer's inventory
            foreach (InventoryItem ii in gameSession.CurrentPlayer.Inventory)
            {
                if (ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            // Display message
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run("You drink a " + potion.Name + Environment.NewLine)));

            // Monster gets their turn to attack

            // Determine the amount of damage the monster does to the CurrentPlayer
            int damageToCurrentPlayer = RandomNumberGenerator.NumberBetween(0, currentMonster.MaximumDamage);

            // Display message
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run("The " + currentMonster.Name + " did " + damageTogameSession.CurrentPlayer.ToString() + " points of damage." + Environment.NewLine)));

            // Subtract damage from CurrentPlayer
            gameSession.CurrentPlayer.HitPoints -= damageToCurrentPlayer;

            if (gameSession.CurrentPlayer.HitPoints <= 0)
            {
                // Display message
                rtbMessages.Document.Blocks.Add(new Paragraph(new Run("The " + currentMonster.Name + " killed you." + Environment.NewLine)));

                // Move CurrentPlayer to "Home"
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            // Refresh CurrentPlayer data in UI
            hitpointsValueLabel.Content = gameSession.CurrentPlayer.HitPoints.ToString();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.HeadersVisibility = DataGridHeadersVisibility.None;

            dgvInventory.Columns.Add(new DataGridTextColumn
            {
                Header = "Name",
                Width = 197,
            });
            dgvInventory.Columns.Add(new DataGridTextColumn
            {
                Header = "Quantity"
            });

            dgvInventory.Items.Clear();

            foreach (InventoryItem inventoryItem in gameSession.CurrentPlayer.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Items.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.HeadersVisibility = DataGridHeadersVisibility.None;

            dgvQuests.Columns.Add(new DataGridTextColumn
            {
                Header = "Name",
                Width = 197,
            });
            dgvQuests.Columns.Add(new DataGridTextColumn
            {
                Header = "Done?"
            });

            foreach (CurrentPlayerQuest CurrentPlayerQuest in gameSession.CurrentPlayer.Quests)
            {
                dgvQuests.Items.Add(new[] { CurrentPlayerQuest.Details.Name, CurrentPlayerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in gameSession.CurrentPlayer.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                // The CurrentPlayer doesn't have any weapons, so hide the weapon combobox and "Use" button
                cboWeapons.Visibility = Visibility.Hidden;
                btnUseWeapon.Visibility = Visibility.Hidden;
            }
            else
            {
                cboWeapons.ItemsSource = weapons;
                cboWeapons.DisplayMemberPath = "Name";
                cboWeapons.SelectedValuePath = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in gameSession.CurrentPlayer.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                // The CurrentPlayer doesn't have any potions, so hide the potion combobox and "Use" button
                cboPotions.Visibility = Visibility.Hidden;
                btnUsePotion.Visibility = Visibility.Hidden;
            }
            else
            {
                cboPotions.ItemsSource = healingPotions;
                cboPotions.DisplayMemberPath = "Name";
                cboPotions.SelectedValuePath = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void UpdateUserInfo()
        {
            hitpointsValueLabel.Content = gameSession.CurrentPlayer.HitPoints.ToString();
            goldValueLabel.Content = gameSession.CurrentPlayer.Gold.ToString();
            experienceValueLabel.Content = gameSession.CurrentPlayer.ExperiencePoints.ToString();
            levelValueLabel.Content = gameSession.CurrentPlayer.Level.ToString();
        }

        private void UpdateNotQuest()
        {
            UpdateInventoryListInUI();
            UpdateWeaponListInUI();
            UpdatePotionListInUI();
        }

        private void SendYouLootMessage(InventoryItem inventoryItem, string item)
        {
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run("You loot " + inventoryItem.Quantity.ToString() + " " + item + Environment.NewLine)));
        }

        private void ReceiveReward(Location newLocation)
        {
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run("You receive: " + Environment.NewLine)));
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run(newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine)));
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run(newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine)));
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run(newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine)));
            AddNewLine();

            gameSession.CurrentPlayer.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
            gameSession.CurrentPlayer.Gold += newLocation.QuestAvailableHere.RewardGold;

            gameSession.CurrentPlayer.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);
        }

        private void CompleteQuest(Location newLocation)
        {
            AddNewLine();
            rtbLocation.Document.Blocks.Add(new Paragraph(new Run("You complete the '" + newLocation.QuestAvailableHere.Name + "' quest." + Environment.NewLine)));

            gameSession.CurrentPlayer.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);
        }

        private void AddNewLine()
        {
            rtbLocation.Document.Blocks.Add(new Paragraph(new Run(Environment.NewLine)));
        }

        private void ReceiveNewQuest(Location newLocation)
        {
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run("You receive the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine)));
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run(newLocation.QuestAvailableHere.Description + Environment.NewLine)));
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run("To complete it, return with:" + Environment.NewLine)));
        }

        private void FightMonster(bool monsterVisible)
        {
            if (monsterVisible)
            {
                cboWeapons.Visibility = Visibility.Visible;
                cboPotions.Visibility = Visibility.Visible;
                btnUseWeapon.Visibility = Visibility.Visible;
                btnUsePotion.Visibility = Visibility.Visible;
            }
            cboWeapons.Visibility = Visibility.Hidden;
            cboPotions.Visibility = Visibility.Hidden;
            btnUseWeapon.Visibility = Visibility.Hidden;
            btnUsePotion.Visibility = Visibility.Hidden;

        }

        private void ToggleButtonVisibility(Location newLocation)
        {
            btnNorth.Visibility = (newLocation.LocationToNorth != null) ? Visibility.Visible : Visibility.Hidden;
            btnEast.Visibility = (newLocation.LocationToEast != null) ? Visibility.Visible : Visibility.Hidden;
            btnSouth.Visibility = (newLocation.LocationToSouth != null) ? Visibility.Visible : Visibility.Hidden;
            btnWest.Visibility = (newLocation.LocationToWest != null) ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
