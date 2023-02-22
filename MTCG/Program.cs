// See https://aka.ms/new-console-template for more information
using MTCG.BL.BattleLogic;
using MTCG.Model.Cards;
using MTCG.Model.User;

Console.WriteLine("Hello, World!");

var user1 = new User("user1", "password");
var user2 = new User("user2", "password");

var card1 = new Card("FireSpell", 10, Card.ElementType.Fire, Card.CardType.Spell);
var card2 = new Card("WaterGoblin", 5, Card.ElementType.Water, Card.CardType.Monster);
var card3 = new Card("WaterSpell", 20, Card.ElementType.Water, Card.CardType.Spell);
var card4 = new Card("FireTroll", 15, Card.ElementType.Fire, Card.CardType.Monster);

var user1Stack = new List<Card> { card1, card2, card3, card4 };
var user2Stack = new List<Card> { card2, card3, card4, card1 };

var user1Deck = new List<Card> { card1, card2, card3, card4 };
var user2Deck = new List<Card> { card2, card3, card4, card1 };

var battle = new Battle(user1Deck, user2Deck);

var battlelog = battle.Start();

foreach (var log in battlelog)
{
    Console.WriteLine(log);
}



