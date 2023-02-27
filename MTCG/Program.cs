// See https://aka.ms/new-console-template for more information
using MTCG.BL.BattleLogic;
using MTCG.DAL;
using MTCG.Model.Cards;
using MTCG.Model.User;


var user1 = new User("user1", "password");
var user2 = new User("user2", "password");

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

var user1Stack = new List<Card> { card1, card2, card3, card4, card5, card6, card7, card8, card9, card10, card11 };
var user2Stack = new List<Card> { card2, card3, card4, card1, card6, card7, card8, card9, card10, card11, card5 };

var user1Deck = new List<Card> { card2, card6, card8, card11 };
var user2Deck = new List<Card> { card1, card4, card5, card7 };

var battle = new Battle(user1Deck, user2Deck);

var battlelog = battle.Start();

foreach (var log in battlelog)
{
    Console.WriteLine(log);
}

DBHandler dBHandler = new DBHandler("Host=localhost;Port=5432;Database=mtcg;Username=swe1user;Password=swe1pw");

dBHandler.SetupConnection();




