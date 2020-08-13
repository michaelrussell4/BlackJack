using System; 
using System.Collections.Generic;

namespace BlackJack 
{
  public enum Suits {
    Clubs, 
    Diamonds, 
    Hearts, 
    Spades
  }
  
  public enum Cards {
    Ace = 1,
    Two, 
    Three, 
    Four, 
    Five, 
    Six, 
    Seven, 
    Eight, 
    Nine, 
    Ten,
    Jack = 10,
    Queen = 10, 
    King = 10
  }
  
  public struct Card {
    public Suits suit {get; set;}
    public Cards card {get; set;}
    
    public Card(Suits suit, Cards card) {
      this.suit = suit;
      this.card = card;
    }
  }
  
  public class Deck {
      
    public List<Card> deck; 
    private Random rand = new Random();
    
    public Deck() {
      ShuffleDeck();
    }
    
    bool DeckCompiled() {
      return deck != default(List<Card>);
    }
    
    public void BuildDeck() {
      deck = new List<Card>();
      foreach(Suits s in Enum.GetValues(typeof(Suits))){
        foreach(Cards c in Enum.GetValues(typeof(Cards))){
          deck.Add(new Card(s, c));
        }
      }
    }
    
    public void ShuffleDeck() {
      if(!DeckCompiled()){
        BuildDeck();
      }
      int n = deck.Count;  
      while (n > 1) {  
        n--;  
        int k = rand.Next(n + 1);  
        Card value = deck[k];  
        deck[k] = deck[n];  
        deck[n] = value;  
      } 
    }
    
    public Card RemoveCard() {
      Card c = deck[0];
      deck.RemoveAt(0);
      return c;
    }
    
    public void PrintDeck() {
      int i = 0;
      foreach(Card c in deck){
          Console.WriteLine("{0}: {1} of {2}", ++i, c.card, c.suit);
      }
    }
  }

  public struct Player {
    public Score score {get; set;}
    public Hand hand {get; set;}
    public bool dealer {get; set;}

    public Player(Hand hand, bool dealer = false) {
      this.dealer = dealer;
      this.hand = hand; 
      score = new Score(-1, false);    
      this.UpdateScore(); 
    }

    public void UpdateScore() {
      int sum = 0;
      bool ace = false;
      foreach(Card card in hand.hand){
        sum += (int)card.card;
        ace |= card.card == Cards.Ace;
      }
      score = new Score(sum, ace);
    }
  }

  public class Hand {
    public List<Card> hand; 

    // Hand must start out with at least two cards
    public Hand(Card c1, Card c2) {
      hand = new List<Card>(new Card[] {c1, c2});
    }
    
    public List<Card> GetHand() {
      return hand;
    }

    public void AddCard(Card c) {
      hand.Add(c);
    }
  }

  public struct Score {
    public int numScore {get; set;}
    public int numAltScore {get; set;}
    public bool ace {get; set;}
    public string strScore {get; set;}

    public Score(int numScore, bool ace) {
      this.numScore = numScore;
      this.ace = ace;
      numAltScore = ace? numScore + 10:numScore;
      strScore = ace? String.Format("{0} or {1}", numScore, numAltScore):numScore.ToString();
    }
  }

  public class Game {    
    public Deck deck;
    public List<Player> hands;
    bool blackJack = false;
    public string boxDealer = @"
      ┌─────────────────┐
      │     Dealer      │
      └─────────────────┘
    ";
    public string boxUser = @"
      ┌─────────────────┐
      │     Player      │
      └─────────────────┘
    ";
    
    public Game() {
      PrintWelcomeScreen();
      InitializeVariables();
      ShowCards();
      while(UserTurn()){
        UserHit();
        foreach(Player player in hands)
          player.UpdateScore();
        if(CheckForBust())
          break;
        if(CheckUserBlackJack())
          break;
        ShowCards();
      }
      DealerTurn();
      ShowWinner();
    }

    void PrintWelcomeScreen() {
      Console.WriteLine(@"
  ╔═════════════════════════════╗
  ║    Welcome to Black Jack    ║
  ╚═════════════════════════════╝
        ");
    }

    void InitializeVariables() {
      deck = new Deck();
      hands = new List<Player>{
        {new Player(new Hand(deck.RemoveCard(), deck.RemoveCard()), true)},
        {new Player(new Hand(deck.RemoveCard(), deck.RemoveCard()))}
      };
    }

    void ShowCards(bool showDealer=false, bool showUser=true) {
      foreach(Player player in hands){
        Console.WriteLine(player.dealer? boxDealer:boxUser);
        player.UpdateScore();
        foreach(Card card in player.hand.hand){
          string message = "\t\t\t" + (!showDealer&&player.dealer? "hidden":
            String.Format("{0} of {1}", card.card.ToString(), card.suit.ToString()));
          Console.WriteLine(message);
        }
        Console.WriteLine("\nScore: {0}",!showDealer&&player.dealer? "hidden":
          player.score.strScore);
      }
    }

    bool CheckForBust() {
      bool bust = false;
      foreach(Player player in hands){
        bust |= player.score.numScore > 21;
      }
      return bust;
    }

    bool UserTurn() {
      int response = -1;
      while(response != 1 && response != 2){
        Console.WriteLine(@"
        What would you like to do? 
          [1] Hit
          [2] Stay
        ");
        string strResponse = Console.ReadLine();
        int.TryParse(strResponse, out response);
      }
      return response==1;
    }

    void UserHit() {
      foreach(Player player in hands){
        if(player.dealer)
          continue;
        player.hand.AddCard(deck.RemoveCard());
        player.UpdateScore();
      }
    }

    void DealerTurn() {
      foreach(Player player in hands){
        if(!player.dealer)
          continue;
        while(player.score.numScore < 17 || 
          (player.score.ace && player.score.numAltScore <= 17)) {
          player.hand.AddCard(deck.RemoveCard());
          player.UpdateScore();
        }
        player.hand.AddCard(deck.RemoveCard());
      }
    }

    bool CheckUserBlackJack() {
      foreach(Player player in hands){
        if(player.dealer)
          continue;
        player.UpdateScore();
        if(player.score.numScore == 21 || player.score.numAltScore == 21){
          blackJack = true;
          return true;
        }
      }
      return false;
    }

    void ShowWinner() {
      ShowCards(true);
      Console.WriteLine("\n...And the winner is...\n");
      int p1=0, p2=0;
      foreach(Player player in hands){
        if(player.dealer){
          p1 = player.score.numAltScore <= 21? player.score.numAltScore:
            player.score.numScore;
        }
        else{
          if(blackJack){
            if(player.score.numScore == 21 || player.score.numAltScore == 21){
              Console.WriteLine(@"
              You Win!
          ┌─────────────────┐
          │    BlackJack!   │
          └─────────────────┘
              ");
            }
            else{
              Console.WriteLine(@"
              You Lose..
          ┌───────────────────────┐
          │ Dealer got BlackJack  │
          └───────────────────────┘
              ");
            }
            return;
          }
          p2 = player.score.numAltScore <= 21? player.score.numAltScore:
            player.score.numScore;
        }
      }
      Console.WriteLine(p1==p2? "It's a tie!":p1>p2? boxDealer:boxUser);
    }
    
    static void Main() {
      new Game();
    }
  }
}