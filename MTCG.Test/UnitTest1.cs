using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTCG.BL.BattleLogic;
using MTCG.Model.Cards;
using System.Collections.Generic;

namespace MTCG.Test
{
    [TestClass]

    public class UnitTest1
    {
        Card card1 = new Card("FireSpell", 10, Card.ElementType.Fire, Card.CardType.Spell);
        Card card2 = new Card("WaterGoblin", 5, Card.ElementType.Water, Card.CardType.Monster);
        Card card3 = new Card("WaterSpell", 20, Card.ElementType.Water, Card.CardType.Spell);
        Card card4 = new Card("FireTroll", 15, Card.ElementType.Fire, Card.CardType.Monster);
        Card card5 = new Card("WaterGoblin", 10, Card.ElementType.Water, Card.CardType.Monster);
        Card card6 = new Card("WaterSpell", 5, Card.ElementType.Water, Card.CardType.Spell);
        Card card7 = new Card("FireSpell", 20, Card.ElementType.Fire, Card.CardType.Spell);
        Card card8 = new Card("FireSpell", 90, Card.ElementType.Fire, Card.CardType.Spell);
        Card card9 = new Card("WaterSpell", 10, Card.ElementType.Water, Card.CardType.Spell);
        Card card10 = new Card("RegularSpell", 10, Card.ElementType.Normal, Card.CardType.Spell);
        Card card11 = new Card("Knight", 15, Card.ElementType.Normal, Card.CardType.Monster);


        Battle battle = new Battle(new List<Card> { new Card("FireSpell", 10, Card.ElementType.Fire, Card.CardType.Spell), new Card("WaterGoblin", 5, Card.ElementType.Water, Card.CardType.Monster), new Card("WaterSpell", 20, Card.ElementType.Water, Card.CardType.Spell), new Card("FireTroll", 15, Card.ElementType.Fire, Card.CardType.Monster) }, new List<Card> { new Card("FireSpell", 10, Card.ElementType.Fire, Card.CardType.Spell), new Card("WaterGoblin", 5, Card.ElementType.Water, Card.CardType.Monster), new Card("WaterSpell", 20, Card.ElementType.Water, Card.CardType.Spell), new Card("FireTroll", 15, Card.ElementType.Fire, Card.CardType.Monster), new Card("FireSpell", 10, Card.ElementType.Fire, Card.CardType.Spell) });
        [TestMethod]
        public void testFireSpellVsWaterGoblin()
        {
            Assert.AreEqual(battle.GetElementEffectiveness(card1.elementType, card2.elementType), 0.5);
        }
        [TestMethod]
        public void testDamageMonsterVsMonster()
        {
            Assert.AreEqual(battle.GetWinner(card5, card4), card4);

        }
        [TestMethod]
        public void testDamageMonsterVsMonster2()
        {
            Assert.AreEqual(battle.GetWinner(card4, card5), card4);

        }

        // Spell Fight Tests

        [TestMethod]
        public void testSpellFights1()
        {
            Assert.AreEqual(battle.GetWinnerWithSpell(card1, card3), card3);
        }

        [TestMethod]
        public void testSpellFights2()
        {
            Assert.AreEqual(battle.GetWinnerWithSpell(card7, card6), null);
        }

        [TestMethod]
        public void testSpellFights3()
        {
            Assert.AreEqual(battle.GetWinnerWithSpell(card8, card6), card8);
        }

        // Mixed Fights

        [TestMethod]
        public void testMixedFight1()
        {
            Assert.AreEqual(battle.GetWinnerWithSpell(card1, card5), card5);
        }

        [TestMethod]
        public void testMixedFight2()
        {
            Assert.AreEqual(battle.GetWinnerWithSpell(card9, card5), null);
        }

        [TestMethod]
        public void testMixedFight3()
        {
            Assert.AreEqual(battle.GetWinnerWithSpell(card10, card5), card10);
        }

        [TestMethod]
        public void testMixedFight4()
        {
            Assert.AreEqual(battle.GetWinnerWithSpell(card10, card11), card11);
        }








    }
}