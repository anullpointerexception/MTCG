using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTCG.BL.BattleLogic;
using MTCG.DAL;
using MTCG.Model.Cards;
using MTCG.Model.User;

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
        Card card12 = new Card("FireSpell", 30, Card.ElementType.Fire, Card.CardType.Spell);
        Card card13 = new Card("FireSpell", 10, Card.ElementType.Fire, Card.CardType.Spell);






        DBHandler dBHandler = DBHandler.Instance;




        Battle battle = new Battle();
        [TestMethod]
        public void testFireSpellVsWaterGoblin()
        {
            Assert.AreEqual(0.5, battle.GetElementEffectiveness(card1.elementType, card2.elementType));
        }
        [TestMethod]
        public void testDamageMonsterVsMonster()
        {
            Assert.AreEqual(card4, battle.GetWinner(card5, card4));

        }
        [TestMethod]
        public void testDamageMonsterVsMonster2()
        {
            Assert.AreEqual(card4, battle.GetWinner(card4, card5));

        }

        [TestMethod]
        public void testSpellVSSpell()
        {
            Assert.AreEqual(card12, battle.GetWinner(card12, card13));

        }

        // Spell Fight Tests

        [TestMethod]
        public void testSpellFights1()
        {
            Assert.AreEqual(card3, battle.GetWinnerWithSpell(card1, card3));
        }

        [TestMethod]
        public void testSpellFights2()
        {
            Assert.AreEqual(null, battle.GetWinnerWithSpell(card7, card6));
        }

        [TestMethod]
        public void testSpellFights3()
        {
            Assert.AreEqual(card8, battle.GetWinnerWithSpell(card8, card6));
        }

        // Mixed Fights

        [TestMethod]
        public void testMixedFight1()
        {
            Assert.AreEqual(card5, battle.GetWinnerWithSpell(card1, card5));
        }

        [TestMethod]
        public void testMixedFight2()
        {
            Assert.AreEqual(null, battle.GetWinnerWithSpell(card9, card5));
        }

        [TestMethod]
        public void testMixedFight3()
        {
            Assert.AreEqual(card10, battle.GetWinnerWithSpell(card10, card5));
        }

        [TestMethod]
        public void testMixedFight4()
        {
            Assert.AreEqual(card11, battle.GetWinnerWithSpell(card10, card11));
        }

        [TestMethod]
        public void testInsertToDB()
        {
            Assert.AreEqual(card11, battle.GetWinnerWithSpell(card10, card11));
        }

        // DB
        // Token
        [TestMethod]
        public void TestDatabase_Authorization_Token_Correct()
        {
            dBHandler.SetupConnection();
            dBHandler.UserToken = "kienboec-mtcgToken";

            Assert.AreEqual(true, dBHandler.AuthorizedUser());
            Assert.AreEqual("kienboec", dBHandler.currentUser);
        }

        [TestMethod]
        public void TestDatabase_Authorization_Token_Incorrect()
        {
            dBHandler.SetupConnection();

            dBHandler.UserToken = "random-mtcgToken";

            Assert.AreEqual(false, dBHandler.AuthorizedUser());
            Assert.AreEqual(null, dBHandler.currentUser);
        }

        [TestMethod]
        public void TestDatabase_User_NotExistingUser()
        {
            dBHandler.SetupConnection();
            AccountData? demoData = dBHandler.getUserFromDB("Maxl");

            Assert.AreEqual(null, demoData);
        }

        [TestMethod]
        public void TestDatabase_Login_InCorrect()
        {
            dBHandler.SetupConnection();
            string? token = dBHandler.Login("kienboec", "wrong");
            Assert.AreEqual(null, token);
        }

        [TestMethod]
        public void TestDatabase_Login_Correct()
        {
            dBHandler.SetupConnection();

            string? token = dBHandler.Login("kienboec", "daniel");
            Assert.AreEqual("kienboec-mtcgToken", token);
        }

        // create package

        [TestMethod]
        public void TestDatabase_CreatePackage_UserIsAdmin()
        {
            dBHandler.SetupConnection();
            dBHandler.UserToken = "admin-mtcgToken";
            dBHandler.AuthorizedUser();

            Assert.AreEqual("admin", dBHandler.currentUser);
            int code = dBHandler.CreateCardPackage();
            Assert.AreEqual(201, code);
        }


        [TestMethod]
        public void TestDatabase_CreatePackage_UserIsNotAdmin()
        {
            dBHandler.SetupConnection();
            dBHandler.UserToken = "someUser-mtcgToken";
            dBHandler.AuthorizedUser();

            Assert.AreNotEqual("admin", dBHandler.currentUser);
            int code = dBHandler.CreateCardPackage();
            Assert.AreEqual(403, code);
        }

        [TestMethod]
        public void TestDatabase_UserData()
        {
            dBHandler.SetupConnection();

            AccountData? kienboec = dBHandler.getUserFromDB("kienboec");

            kienboec.Bio = "NewBio";
            kienboec.Image = ":)";
            kienboec.Name = "Karl";

            dBHandler.UpdateAccount(kienboec);

            kienboec = dBHandler.getUserFromDB("kienboec");

            Assert.AreEqual("NewBio", kienboec.Bio);
            Assert.AreEqual(":)", kienboec.Image);
            Assert.AreEqual("Karl", kienboec.Name);
        }
















    }
}